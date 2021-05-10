using UnityEngine;

/// <summary>
/// Positions self behind a car and provides 
/// </summary>
public class CarCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] Vector3 _boomOffset = new Vector3(0, 2, -10);
    [SerializeField] float _boomPitch = 15f;
    [SerializeField] float _rotationAcceleration = 45f;
    [SerializeField] float _accelerationEstimationFactor = 0.75f; //Pretend like we're this ratio of our actual acceleration to reduce overshoot.

    [Header("Runtime / Active Target")]
    [SerializeField] Transform _followTarget = null;

    float _yawVelocity;

    // Copy-pasta from Astro Pirates physics code.
    static float GetTargetVelocity(float deltaTarget, float acceleration)
    {
        return Mathf.Sign(deltaTarget) * Mathf.Sqrt(acceleration * 2f * Mathf.Abs(deltaTarget));
    }

    void Start()
    {
        // Clear color between frames so it's easier to debug in Renderdoc
        Camera cam = gameObject.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
    }

    void LateUpdate()
    {
        // Can't do anything if we don't have target.
        if (_followTarget == null)
        {
            _yawVelocity = 0f;
            return;
        }

        float deltaTime = Time.deltaTime;

        // Figure out the pivot point of the camera's arm/boom.
        var pivotPosition = _followTarget.position;

        
        var up = Vector3.up;

        // Figure out forward directions in current up plane.
        var targetForward = Vector3.ProjectOnPlane(_followTarget.forward, up);
        var currentForward = Vector3.ProjectOnPlane(transform.forward, up);

        // Make sure our pitch/roll is aligned correctly, but don't modify yaw yet.
        var rotation = Quaternion.LookRotation(currentForward, up)*Quaternion.AngleAxis(_boomPitch, Vector3.right);

        // Figure out how much our yaw is off by.
        float yawOffset = Vector3.SignedAngle(currentForward, targetForward, up);

        // Estimate target yaw velocity and move our yaw velocity towards it.
        float targetYawVelocity = GetTargetVelocity(yawOffset, _rotationAcceleration * _accelerationEstimationFactor);
        _yawVelocity = Mathf.MoveTowards(_yawVelocity, targetYawVelocity, _rotationAcceleration * deltaTime);

        // Rotate based on our yaw velocity.
        rotation *= Quaternion.AngleAxis(_yawVelocity * deltaTime, Vector3.up);

        var position = pivotPosition + rotation * _boomOffset;

        transform.SetPositionAndRotation(position, rotation);
    }
}
