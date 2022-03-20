using UnityEngine;
using System.Collections;
using SplineMesh;

public class CarPhysicsObject : VehicleBase
{
    [SerializeField] CarPhysicsSettings _settings = default;

    [SerializeField] bool _autoDrive = false;

    [SerializeField] float _autoDriveSpeed = 0.05f;

    [SerializeField]
    AudioSource _engineSound;

    Rigidbody _body;

    float _currentSteering;

    public SkinnedMeshRenderer _carRenderer;

    // Boost
    float BoostVelMultiplier = 1.0f;
    float BoostAccelMultiplier = 1.0f;
    float BoostStartTime = -1.0f;

    public struct CarVisualData
    {
        public SkinnedMeshRenderer renderer;
        public Transform leftFrontWheel;
        public Transform rightFrontWheel;
        public Transform leftBackWheel;
        public Transform rightBackWheel;
        public float wheelSteering;
        public float wheelSpin;

        public GameObject SpeedBoostFX;
        public CarCamera carCam;
    }

    public CarVisualData _visualData;

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
        _visualData.renderer = _carRenderer;// GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;

        _visualData.SpeedBoostFX = RecursiveFindChild(gameObject.transform, "SpeedBoostFX").gameObject;

        GameObject carCamRig = GameObject.Find("CarCameraRig");
        if (carCamRig != null)
        {
            _visualData.carCam = carCamRig.GetComponent<CarCamera>();
        }
    }

    public void SetEngineVolume(float newVolume)
    {
        _engineSound.volume = newVolume;
    }

    public void StartCar()
    {
        gameObject.SetActive(true);
        GameObject road = GameObject.Find("ProceduralRoadPiece(Clone)");
        var spline = road.GetComponent<Spline>();
        var curveSample = spline.GetProjectionSample(transform.position);
        DistanceAlongSpline = curveSample.distanceAlongSpline;
    }
    public void StopCar()
    {
        _engineSound.pitch = 0;
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (_autoDrive)
        {
            AutoDrive();
            UpdateVisuals(deltaTime, Vector3.zero);
            return;
        }

        // OXIDE BEGIN - bwilson - todo
        if (Input.Brake > 0)
        {
            _body.velocity *= 0.95f;
        }
//        Debug.Log(Time.time + " -> " + _body.velocity);
        _engineSound.pitch = 2.0f * (_body.velocity.z / _settings.TopSpeed);
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

        float topSpeed = _settings.TopSpeed * BoostVelMultiplier;
        float topAccel = _settings.Acceleration * BoostAccelMultiplier;

        float maxWheelAngle = Mathf.Lerp(_settings.TurnAngleLowSpeed, _settings.TurnAngleTopSpeed, horizontalVelocity.magnitude / topSpeed);
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

        var targetForwardVelocity = steeringForward * Mathf.Clamp(Input.Gas, -1, 1) * topSpeed;

        // Accelerate!
        forwardVelocity = Vector3.MoveTowards(forwardVelocity, targetForwardVelocity, topAccel * deltaTime);

        // Reduce our drift based on drift friction and our new forward speed.

        float driftSpeedMeasure = Mathf.Max(drift.magnitude, forwardVelocity.magnitude);
        drift = Vector3.MoveTowards(drift, default, _settings.DriftFriction*driftSpeedMeasure * deltaTime);

        // Re-combine our forward velocity and drift into our velocity.
        velocity += forwardVelocity + drift;

        var localAngularVelocity = Quaternion.Inverse(rotation) * angularVelocity * Mathf.Rad2Deg;

        // Instantly settings rotation speed based on correction.

        float rotationSpeedMultiplier = Mathf.Clamp01(driftSpeedMeasure / topSpeed);
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
         //   Debug.Log(Time.time + "Dist Traveled = " + DistanceAlongSpline + ", normalized dist traveled = " + DistanceAlongSpline / spline.Length);
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
            int yellPicker = Random.Range((int)0, 2);

            if (yellPicker == 0)
            {
                GameController.GetGameController().PlayDrivingBanter("PinkyYell1");
            }
            else if(yellPicker == 1)
            {
                GameController.GetGameController().PlayDrivingBanter("StiltzYell1");
            }
            //GameController.GetGameController().PlayDrivingBanter("HeadSway");
        }
    }

    float distance = 0;
    Vector3 targetDir;

    public void CheatWarp(float dist)
    {
        GameObject road = GameObject.Find("ProceduralRoadPiece(Clone)");
        var spline = road.GetComponent<Spline>();
        var theLoc = spline.GetSampleAtDistance(dist);
        var roadTangent = Vector3.ProjectOnPlane(theLoc.tangent, Vector3.up);
        roadTangent = Vector3.Cross(roadTangent, Vector3.up);

        var roadPoint = road.transform.TransformPoint(theLoc.location);
        transform.position = roadPoint;
        //        Debug.DrawLine(roadPoint, roadPoint + new Vector3(0, 1000, 0)); 
    }

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

    public void StartSpeedBoost(float VelMultiplier, float AccelMultiplier, float BoostLengthSec)
    {
        BoostVelMultiplier = VelMultiplier;
        BoostAccelMultiplier = AccelMultiplier;

        StartCoroutine(SpeedBoost(BoostLengthSec));
    }

    IEnumerator SpeedBoost(float BoostLengthSec)
    {
        //        Debug.Log("Woot!");
        BoostStartTime = Time.realtimeSinceStartup;
        _visualData.carCam._boomOffsetLerp = 0.0f;

        _visualData.SpeedBoostFX.SetActive(true);

        float startTime = Time.realtimeSinceStartup;
        float zoomOutLength = 0.2f;
        float finalTime = startTime + BoostLengthSec;
        float boomMax = 0.1f;
        while (Time.realtimeSinceStartup < startTime + zoomOutLength)
        {
            float lerpAmt = Mathf.Clamp((Time.realtimeSinceStartup - startTime) / zoomOutLength, 0, 1.0f);
            _visualData.carCam._boomOffsetLerp = lerpAmt;
            yield return null;
        }
        _visualData.carCam._boomOffsetLerp = 1.0f;

        float zoomInLength = zoomOutLength * 2.0f;
        yield return new WaitForSeconds(finalTime - zoomInLength - Time.realtimeSinceStartup);

        startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < finalTime)
        {
            float lerpAmt = 1.0f - Mathf.Clamp((Time.realtimeSinceStartup - startTime) / zoomInLength, 0, 1.0f);
            _visualData.carCam._boomOffsetLerp = lerpAmt;
            yield return null;
        }
        _visualData.carCam._boomOffsetLerp = 0.0f;

        BoostVelMultiplier = 1;
        BoostAccelMultiplier = 1;

        _visualData.SpeedBoostFX.SetActive(false);
    }
}
