using UnityEngine;

/// <summary>
/// Positions self behind a car and provides 
/// </summary>
public class CarCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] Vector3 _boomOffset = new Vector3(0, 2, -10);
    [SerializeField] float _boomPitch = 15f;
    [SerializeField] float _rotationLinearSpeed = 2f;
    [SerializeField] float _rotationLerpSpeed = 20f;

    [Header("Runtime / Active Target")]
    [SerializeField] Transform _followTarget = null;

    void LateUpdate()
    {
        if (_followTarget == null)
            return;

        float deltaTime = Time.deltaTime;

        // Figure out the pivot point of the camera's arm/boom.
        var pivotPosition = _followTarget.position;
        var targetRotation = _followTarget.rotation * Quaternion.AngleAxis(_boomPitch, Vector3.right);

        var rotation = transform.rotation;
        // Smoothly move it towards the target, but also do the RotateTowards so we still reach it.
        rotation = Quaternion.Lerp(rotation, targetRotation, _rotationLerpSpeed * deltaTime);
        rotation = Quaternion.RotateTowards(rotation, targetRotation, _rotationLinearSpeed * deltaTime);

        var position = pivotPosition + rotation * _boomOffset;

        transform.SetPositionAndRotation(position, rotation);
    }
}
