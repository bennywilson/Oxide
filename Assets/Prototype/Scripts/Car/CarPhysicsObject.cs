using UnityEngine;

public class CarPhysicsObject : VehicleBase
{
    [SerializeField] CarPhysicsSettings _settings = default;

    Rigidbody _body;

    float _currentSteering;

    void Start()
    {
        _body = GetComponent<Rigidbody>();

        if (_body == null)
        {
            enabled = false;
            Debug.LogErrorFormat("{0} needs a Rigidbody component to function!", name);
        }
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

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
    }
}
