using UnityEngine;
using System.Collections;
using SplineMesh;

public class CarPhysicsObject : VehicleBase
{
    [SerializeField] CarPhysicsSettings _settings = default;

    [SerializeField] bool _autoDrive = false;

    [SerializeField] float _autoDriveSpeed = 0.05f;

    [SerializeField]
    GameObject _curseTextBubble1;

    [SerializeField]
    GameObject _curseTextBubble2;

    Rigidbody _body;

    float _currentSteering;

    public struct CarVisualData
    {
        public SkinnedMeshRenderer renderer;
        public Transform leftFrontWheel;
        public Transform rightFrontWheel;
        public Transform leftBackWheel;
        public Transform rightBackWheel;
        public float wheelSteering;
        public float wheelSpin;
    }

    CarVisualData _visualData;

    Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        } 
        return null;
    }

    void Start()
    {
        _body = GetComponent<Rigidbody>();

        if (_body == null)
        {
            enabled = false;
            Debug.LogErrorFormat("{0} needs a Rigidbody component to function!", name);
        }

        //    GameObject parent = transform.parent.gameObject;
        _visualData.leftFrontWheel = RecursiveFindChild(gameObject.transform, "LWheel").transform;
        _visualData.rightFrontWheel = RecursiveFindChild(gameObject.transform, "RWheel").transform;
        _visualData.leftBackWheel = RecursiveFindChild(gameObject.transform, "LBack").transform;
        _visualData.rightBackWheel = RecursiveFindChild(gameObject.transform, "RBack").transform;
        _visualData.renderer = GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (_autoDrive)
        {
            AutoDrive();
            return;
        }

        // OXIDE BEGIN - bwilson - todo
        if (Input.Brake > 0)
        {
            _body.velocity *= 0.95f;
        }
        // OXIDE - END

        // As a -1 to 1 ratio relative to turn angle, which way do our wheels point now?
        _currentSteering = Mathf.MoveTowards(_currentSteering, Mathf.Clamp(Input.Steering, -1, 1), _settings.SteeringSpeed * deltaTime);

        // Cache some values so we can modify them without expensive side-effects.
        var rotation = _body.rotation;
        var velocity = _body.velocity;
        var up = rotation * Vector3.up;
        var angularVelocity = _body.angularVelocity;

        // Figure out what part of our velocity can be affected by our wheels.
        var horizontalVelocity = Vector3.ProjectOnPlane(velocity, up);
        velocity -= horizontalVelocity;

        float maxWheelAngle = Mathf.Lerp(_settings.TurnAngleLowSpeed, _settings.TurnAngleTopSpeed, horizontalVelocity.magnitude / _settings.TopSpeed);
        var wheelTurnRotation = Quaternion.AngleAxis(_currentSteering * maxWheelAngle, Vector3.up);
        var steeringForward = rotation * wheelTurnRotation * Vector3.forward;

        if (Mathf.Abs(Vector3.Dot(steeringForward, Vector3.forward)) < 0.1f)
        {
            if (steeringForward.x < 0)
            {
                steeringForward = new Vector3(-0.9934924f, 0.0f, 0.1f);
            }
            else
            {
                steeringForward = new Vector3(0.9934924f, 0.0f, 0.1f);
            }
            steeringForward.Normalize();
         }
            // Figure out velocity relative to where our wheels are facing now.
            var forwardVelocity = Vector3.Project(horizontalVelocity, steeringForward);
        var drift = horizontalVelocity - forwardVelocity;

        var targetForwardVelocity = steeringForward * Mathf.Clamp(Input.Gas, -1, 1) * _settings.TopSpeed;

        // Accelerate!
        forwardVelocity = Vector3.MoveTowards(forwardVelocity, targetForwardVelocity, _settings.Acceleration * deltaTime);

        // Reduce our drift based on drift friction and our new forward speed.

        float driftSpeedMeasure = Mathf.Max(drift.magnitude, forwardVelocity.magnitude);
        drift = Vector3.MoveTowards(drift, default, _settings.DriftFriction*driftSpeedMeasure * deltaTime);

        // Re-combine our forward velocity and drift into our velocity.
        velocity += forwardVelocity + drift;

        var localAngularVelocity = Quaternion.Inverse(rotation) * angularVelocity * Mathf.Rad2Deg;

        // Instantly settings rotation speed based on correction.

        float rotationSpeedMultiplier = Mathf.Clamp01(driftSpeedMeasure / _settings.TopSpeed);
        localAngularVelocity.y = Vector3.SignedAngle(rotation*Vector3.forward, steeringForward, up)*_settings.CarRotationSpeedPerUnitOfOffset*rotationSpeedMultiplier;

        // Turn local angular velocity back into the global stuff.
        angularVelocity = _body.rotation * localAngularVelocity * Mathf.Deg2Rad;

        // Finally apply all of it back to the Rigidbody.
        _body.velocity = velocity;
        _body.angularVelocity = angularVelocity;

        UpdateVisuals(deltaTime, velocity);

        GameObject road = GameObject.Find("ProceduralRoadPiece(Clone)");
        var spline = road.GetComponent<Spline>();
        var curveSample = spline.GetProjectionSample(transform.position);
        DistanceAlongSpline = curveSample.distanceAlongSpline;

        Shader.SetGlobalFloat("_DistanceTravelled", DistanceAlongSpline / spline.Length);
        Debug.Log(Time.time + "Dist Trave = " + DistanceAlongSpline / spline.Length);
        /*  var theLoc = spline.GetSampleAtDistance(DistanceAlongSpline);
          var roadTangent = Vector3.ProjectOnPlane(theLoc.tangent, up);
          roadTangent = Vector3.Cross(roadTangent, up);

          var roadPoint = road.transform.TransformPoint(theLoc.location);
          Debug.DrawLine(roadPoint, roadPoint + new Vector3(0, 1000, 0));*/
    }

    void UpdateVisuals(float deltaTime, Vector3 velocity)
    {
        _visualData.wheelSteering = Mathf.MoveTowards(_visualData.wheelSteering, Mathf.Clamp(Input.Steering, -1, 1), deltaTime * 5.0f);
        _visualData.wheelSpin += deltaTime * velocity.magnitude * 500;
        float wheelYaw = _visualData.wheelSteering * 25.0f;
        _visualData.rightFrontWheel.localEulerAngles = new Vector3(-wheelYaw, 0, wheelYaw);
        _visualData.leftFrontWheel.localEulerAngles = new Vector3(-wheelYaw, 0, -wheelYaw);
        
        _visualData.rightFrontWheel.Rotate(new Vector3(0.0f, _visualData.wheelSpin, 0.0f));
        _visualData.leftFrontWheel.Rotate(new Vector3(0.0f, _visualData.wheelSpin, 0.0f));

        _visualData.leftBackWheel.Rotate(new Vector3(0.0f, _visualData.wheelSpin, 0.0f));
        _visualData.rightBackWheel.Rotate(new Vector3(0.0f, _visualData.wheelSpin, 0.0f));

        if (Input.Brake > 0)
        {
            _visualData.renderer.materials[2].SetColor("ColorMultiplier", new Color(55, 1, 1));
        }
        else
        {
            _visualData.renderer.materials[2].SetColor("ColorMultiplier", new Color(1, 1, 1));
        }

        if (Input.WantsToPurr)
        {
           // Debug.Log("prrr");
            Input.WantsToPurr = false;

            var anim = Passenger.GetComponentInChildren<Animation>();
            anim.enabled = true;
            anim.Play();
            StartCoroutine("StiltzCurse");
        }
    }

    private IEnumerator StiltzCurse()
    {
     //   while (true)
        {
            yield return new WaitForSeconds(0.65f);
            _curseTextBubble1.active = true;
            yield return new WaitForSeconds(0.45f);
            _curseTextBubble1.active = false;
            yield return new WaitForSeconds(0.25f);
            _curseTextBubble2.active = true;
            yield return new WaitForSeconds(0.55f);
            _curseTextBubble2.active = false;
        }
    }

    float distance = 0;
    Vector3 targetDir;

    void AutoDrive()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Rigidbody.Destroy(rb);
        }

        GameObject road = GameObject.Find("ProceduralRoadPiece(Clone)");
        var spline = road.GetComponent<Spline>();
        // For our generated road segment, seed a bunch of props.
        if (spline != null)
        {
            distance += _autoDriveSpeed * Time.fixedDeltaTime;
            var sample = spline.GetSampleAtDistance(distance);
            targetDir = Vector3.ProjectOnPlane(sample.tangent, Vector3.up);
            Vector3 curDir = transform.rotation * Vector3.forward;

            curDir = Vector3.MoveTowards(curDir, targetDir, 0.01f);

            Vector3 tangent = Vector3.Cross(curDir, Vector3.up);
            tangent.y = 0.0f;
            tangent.Normalize();
            var roadPoint = road.transform.TransformPoint(sample.location) - tangent * 0.77f;

            transform.position = roadPoint;
            transform.rotation = Quaternion.LookRotation(curDir);
        }
    }
}
