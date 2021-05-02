using SplineMesh;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] GameObject _roadPrefab = null;
    [SerializeField] float _randomOffsetX = 10f;
    [SerializeField] float _segmentSpacing = 10f;
    [SerializeField] int _segments = 100;

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

        MakeRoadPiece(transform.position, rotation, points);
    }
}
