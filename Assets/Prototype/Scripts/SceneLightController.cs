using UnityEngine;

/// <summary>
/// Day/night cycle controller
/// </summary>
public class SceneLightController : MonoBehaviour
{
    [SerializeField] float _cycleDurationSeconds = 30f;
    [SerializeField] float _cycleStartRatio = 0.5f;

    [SerializeField] Color _startColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] Color _endColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    [SerializeField] Light _lightSun = null;

    float _currentCycleDuration = 0f;

    Vector3 forward = Vector3.forward;

    void Start()
    {
        forward = transform.forward;
        _currentCycleDuration = _cycleStartRatio * _cycleDurationSeconds;

        gameObject.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    }

    void Update()
    {
        _currentCycleDuration += Time.deltaTime;
        float lerpAmt = Mathf.Clamp(_currentCycleDuration / _cycleDurationSeconds, 0.0f, 1.0f);

        Color currentLightColor = Color.Lerp(_startColor, _endColor, lerpAmt);

        _lightSun.intensity = currentLightColor.a;
        _lightSun.color = new Color(currentLightColor.r, currentLightColor.g, currentLightColor.b, 1.0f);
        Shader.SetGlobalFloat("_TimeOfDay", lerpAmt);
    }
}
