using SplineMesh;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] bool _generateNewTrackAtRuntime = false;
    [SerializeField] bool _generateNewTrackNow = false;

    [SerializeField] GameObject _roadPrefab = null;
    [SerializeField] float _randomOffsetX = 10f;
    [SerializeField] float _segmentSpacing = 10f;
    [SerializeField] int _segments = 100;

    [SerializeField] GameObject[] _propPrefabs = null;

    [SerializeField] float _propOffsetFromRoad = 2f;
    [SerializeField] float _propRandomOffset = 0.5f;
    [SerializeField] float _minPropSpacing = 0.75f;
    [SerializeField] float _maxPropSpacing = 1f;

    [SerializeField] GameObject[] _carPrefabs = null;
    [SerializeField] float _minCarSpacing = 5f;
    [SerializeField] float _maxCarSpacing = 10f;

    GameObject _generatedWorldObjects = null;
    GameObject _road = null;

    public void MakeRoadPiece(Vector3 position, Quaternion rotation, List<Vector3> localPoints)
    {
        if (_roadPrefab == null)
        {
            Debug.LogErrorFormat("WorldBuilder::MakeRoadPiece: Can't because no road prefab!");
            return;
        }

        if (localPoints == null || localPoints.Count < 2)
        {
            Debug.LogErrorFormat("WorldBuilder::MakeRoadPiece: Can't unless we have at least 2 points!");
            return;
        }

        _road = Instantiate(_roadPrefab, position, rotation, _generatedWorldObjects.transform);

        var spline = _road.GetComponent<Spline>();
        if (spline == null)
        {
            Debug.LogErrorFormat("WorldBuilder::MakeRoadPiece: Can't configure it unless we have a Spline component!");
        }
        else
        {
            spline.nodes.Clear();

            Vector3 previousPoint = default;

            for (int i = 0; i < localPoints.Count; i++)
            {
                var currentPoint = localPoints[i];

                Vector3 nextPoint;

                if (i < localPoints.Count - 1)
                {
                    nextPoint = localPoints[i + 1]; // Point towards next node.
                }
                else
                {
                    nextPoint = currentPoint * 2 - previousPoint; // Make a point away from our previous point for the end node's direction.
                }

                spline.nodes.Add(new SplineNode(currentPoint, currentPoint + (nextPoint - currentPoint)*0.25f));
                previousPoint = currentPoint;
            }

            spline.RefreshCurves();

            var roadPiece = spline.GetComponent<RoadSpline>();
            if (roadPiece != null)
            {
                roadPiece.updateInPlayMode = true;
                roadPiece.MarkDirty();

                // Make sure road has enough segments.
                for (int i = roadPiece.segments.Count; i < spline.nodes.Count; i++)
                {
                    TrackSegment newSegment = new TrackSegment();
                    roadPiece.segments[0].CopySegment(newSegment);
                    roadPiece.segments.Add(newSegment);
                }
            }
        }
    }

    GameObject MakeRoadsideProp(Vector3 position, Quaternion rotation)
    {
        if (_propPrefabs == null)
            return null;

        var prop = _propPrefabs[Random.Range(0, _propPrefabs.Length)];
        if (prop == null)
            return null;

        GameObject gameObj = Instantiate(prop, position, rotation, _generatedWorldObjects.transform);
        return gameObj;
    }

    void PlaceCar(Vector3 position, Quaternion rotation)
    {
        if (_carPrefabs == null)
            return;

        var car = _carPrefabs[Random.Range(0, _carPrefabs.Length)];
        if (car == null)
            return;

        GameObject spawnedCar = Instantiate(car, position, rotation, _generatedWorldObjects.transform);
        MeshRenderer Mesh = spawnedCar.GetComponentInChildren<MeshRenderer>();

        Vector3 randomColor = Random.insideUnitSphere;
        randomColor.Normalize();

        Mesh.materials[0].SetVector("BaseColor", randomColor);
    }

    Vector3 RandomHorizontalOffset(float maxRadius)
    {
        var randomInside = Random.insideUnitCircle;

        return new Vector3(randomInside.x*maxRadius, 0, randomInside.y*maxRadius);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_generateNewTrackNow)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                ClearTrack();
                BuildTrack(false);
            };
        }
        _generateNewTrackNow = false;
    }
#endif

    void Start()
    {
        if (_generateNewTrackAtRuntime)
        {
            ClearTrack();
            BuildTrack(true);
        }
        else
        {
            Transform genWorldObjXForm = gameObject.transform.Find("Generated World Objects");
            if (genWorldObjXForm != null)
            {
                _generatedWorldObjects = genWorldObjXForm.gameObject;
                Transform roadXForm = _generatedWorldObjects.transform.Find("ProceduralRoadPiece(Clone)");
                if (roadXForm != null)
                {
                    _road = roadXForm.gameObject;
                }
            }
        }

        var up = Vector3.up;

        if (_road != null)
        {
            var spline = _road.GetComponent<Spline>();
            // For our generated road segment, seed a bunch of props.
            if (spline != null)
            {
                float length = spline.Length;
                for (float distance = 0; distance < length; distance += Random.Range(_minCarSpacing, _maxCarSpacing))
                {
                    var sample = spline.GetSampleAtDistance(distance);
                    var roadTangent = Vector3.ProjectOnPlane(sample.tangent, up);
                    roadTangent = Vector3.Cross(roadTangent, up);

                    var roadPoint = _road.transform.TransformPoint(sample.location);

                    // To the right
                    PlaceCar(roadPoint + RandomHorizontalOffset(1), Quaternion.LookRotation(roadTangent));
                }
            }
        }
    }

    void ClearTrack()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Generated World Objects")
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        _generatedWorldObjects = new GameObject();
        _generatedWorldObjects.name = "Generated World Objects";
        _generatedWorldObjects.transform.parent = transform;
    }

    void BuildTrack(bool bAutoPlaceProps)
    {
        ClearTrack();

        var points = new List<Vector3>();

        var position = transform.position;
        var rotation = transform.rotation;

        var lastPoint = position;
        var forward = rotation * Vector3.forward;

        for (int i = 0; i < _segments; i++)
        {
            var nextPoint = lastPoint + forward * _segmentSpacing;
            var placeRotation = Quaternion.LookRotation(nextPoint - lastPoint);

            nextPoint += placeRotation * Vector3.right * Random.Range(-_randomOffsetX, _randomOffsetX);
            points.Add(nextPoint);
            lastPoint = nextPoint;
            forward = placeRotation * Vector3.forward;
        }

        MakeRoadPiece(transform.position, rotation, points);

        if (bAutoPlaceProps)
        {
            var up = Vector3.up;

            var spline = _road.GetComponent<Spline>();
            // For our generated road segment, seed a bunch of props.
            if (spline != null)
            {
                float length = spline.Length;
                for (float distance = 0; distance < length; distance += Random.Range(_minPropSpacing, _maxPropSpacing))
                {
                    var sample = spline.GetSampleAtDistance(distance);
                    var roadTangent = Vector3.ProjectOnPlane(sample.tangent, up);
                    roadTangent = Vector3.Cross(roadTangent, up);

                    var roadPoint = _road.transform.TransformPoint(sample.location);

                    // To the right
                    MakeRoadsideProp(roadPoint + (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value * 360f, up));

                    // To the left
                    MakeRoadsideProp(roadPoint - (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value * 360f, up));
                }
            }
        }
    }

    public void PlacePropAlongSpline(int startIdx, int endIdx, GameObject Prop)
    {
        float startDist = 0;
        _road = GameObject.Find("ProceduralRoadPiece(Clone)");
        _generatedWorldObjects = GameObject.Find("Generated World Objects");

        var spline = _road.GetComponent<Spline>();
        int i = 0;
        for (i = 0; i < startIdx; i++)
        {
            startDist += spline.curves[i].Length;
        }

        float endDist = startDist;
        for (; i < endIdx; i++)
        {
            endDist += spline.curves[i].Length;
        }

        var up = Vector3.up;
        for (float distance = startDist; distance < endDist; distance += Random.Range(_minPropSpacing, _maxPropSpacing))
        {
            var sample = spline.GetSampleAtDistance(distance);
            var roadTangent = Vector3.ProjectOnPlane(sample.tangent, up);
            roadTangent = Vector3.Cross(roadTangent, up);

            var roadPoint = _road.transform.TransformPoint(sample.location);

            // To the right
            GameObject newObj = MakeRoadsideProp(roadPoint + (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value * 360f, up));
            Undo.RegisterCreatedObjectUndo(newObj, "Create object");

            // To the left
            newObj = MakeRoadsideProp(roadPoint - (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value * 360f, up));
            Undo.RegisterCreatedObjectUndo(newObj, "Create object");
        }
    }
}
