using SplineMesh;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] GameObject _roadPrefab = null;
    [SerializeField] float _randomOffsetX = 10f;
    [SerializeField] float _segmentSpacing = 10f;
    [SerializeField] int _segments = 100;

    [SerializeField] GameObject[] _propPrefabs = null;

    [SerializeField] float _propOffsetFromRoad = 2f;
    [SerializeField] float _propRandomOffset = 0.5f;
    [SerializeField] float _minPropSpacing = 0.75f;
    [SerializeField] float _maxPropSpacing = 1f;

    public GameObject MakeRoadPiece(Vector3 position, Quaternion rotation, List<Vector3> localPoints)
    {
        if (_roadPrefab == null)
        {
            Debug.LogErrorFormat("WorldBuilder::MakeRoadPiece: Can't because no road prefab!");
            return null;
        }

        if (localPoints == null || localPoints.Count < 2)
        {
            Debug.LogErrorFormat("WorldBuilder::MakeRoadPiece: Can't unlesswe have at least 2 points!");
            return null;
        }

        var road = Instantiate(_roadPrefab, position, rotation);

        var spline = road.GetComponent<Spline>();
        if (spline == null)
        {
            Debug.LogErrorFormat("WorldBuilder::MAkeRoadPiece: Can't configure it unless we have a Spline component!");
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
                    roadPiece.segments.Add(new TrackSegment());
                }
            }
        }

        return road;
    }

    void MakeRoadsideProp(Vector3 position, Quaternion rotation)
    {
        if (_propPrefabs == null)
            return;

        var prop = _propPrefabs[Random.Range(0, _propPrefabs.Length)];
        if (prop == null)
            return;

        Instantiate(prop, position, rotation);
    }

    Vector3 RandomHorizontalOffset(float maxRadius)
    {
        var randomInside = Random.insideUnitCircle;

        return new Vector3(randomInside.x*maxRadius, 0, randomInside.y*maxRadius);
    }

    void Start()
    {
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

        var road = MakeRoadPiece(transform.position, rotation, points);
        var up = Vector3.up;

        var spline = road.GetComponent<Spline>();
        // For our generated road segment, seed a bunch of props.
        if (spline != null)
        {
            float length = spline.Length;
            for (float distance = 0; distance < length; distance += Random.Range(_minPropSpacing, _maxPropSpacing))
            {
                var sample = spline.GetSampleAtDistance(distance);
                var roadTangent = Vector3.ProjectOnPlane(sample.tangent, up);
                roadTangent = Vector3.Cross(roadTangent, up);

                var roadPoint = road.transform.TransformPoint(sample.location);

                // To the right
                MakeRoadsideProp(roadPoint + (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value*360f, up));

                // To the left
                MakeRoadsideProp(roadPoint - (roadTangent * _propOffsetFromRoad) + RandomHorizontalOffset(_propRandomOffset), Quaternion.AngleAxis(Random.value * 360f, up));
            }
        }
    }
}
