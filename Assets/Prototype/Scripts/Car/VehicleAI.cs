using UnityEngine;
using SplineMesh;

public class VehicleAI : MonoBehaviour
{
    [SerializeField]
    float _laneSwitchMinSpeed = 1;

    [SerializeField]
    float _laneSwitchMaxSpeed = 2;

    float _laneSwitchSpeed = 1;

    public float _distance;
    float _curDriveSpeed = 1;
    Vector3 _targetDir;
    float _laneOffset;
    enum MovementPatten
    {
        DriveStraight,
        ZigZag,
        Num
    };

    MovementPatten _movementPattern;

    // Start is called before the first frame update
    void Start()
    {
        _laneOffset = Random.Range(-1.0f, 1.0f);
        _movementPattern = (MovementPatten)Random.Range(0, (int)MovementPatten.Num);
        _laneSwitchSpeed = Random.Range(_laneSwitchMinSpeed, _laneSwitchMaxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
     /*   Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Rigidbody.Destroy(rb);
        }*/

        GameObject road = GameObject.Find("ProceduralRoadPiece(Clone)");
        var spline = road.GetComponent<Spline>();

        // For our generated road segment, seed a bunch of props.
        if (spline != null)
        {
            _distance += _curDriveSpeed * Time.fixedDeltaTime;
            if (_distance >= spline.Length)
            {
                return;
            }
    
            var sample = spline.GetSampleAtDistance(_distance);
            _targetDir = Vector3.ProjectOnPlane(sample.tangent, Vector3.up);
            Vector3 curDir = transform.rotation * Vector3.forward;

            curDir = Vector3.MoveTowards(curDir, _targetDir, 0.01f);

            switch (_movementPattern)
            {
                case MovementPatten.DriveStraight:
                    {
                        break;
                    }

                case MovementPatten.ZigZag:
                    {
                        float zigZagRate = 1;
                        _laneOffset = Mathf.Sin(Time.time * _laneSwitchSpeed) * 0.77f;
                        break;
                    }
            }    
            Vector3 tangent = Vector3.Cross(curDir, Vector3.up);
            tangent.y = 0.0f;
            tangent.Normalize();
            var roadPoint = road.transform.TransformPoint(sample.location) - tangent * _laneOffset;

            transform.position = roadPoint;
            transform.rotation = Quaternion.LookRotation(curDir);
        }
    }
}
