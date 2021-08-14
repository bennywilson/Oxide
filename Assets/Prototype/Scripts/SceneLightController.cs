using UnityEngine;

/// <summary>
/// Day/night cycle controller
/// </summary>
public class SceneLightController : MonoBehaviour
{
    [SerializeField] float _cycleStartDelay = 5f;
    [SerializeField] float _cycleDurationSeconds = 30f;
    [SerializeField] float _cycleStartRatio = 0.5f;

    [SerializeField] Color _startSunColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] Color _EndSunColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    [SerializeField] Color _startBackgroundTintColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] Color _endBackgroundTintColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    [SerializeField] Light _lightSun = null;

    [SerializeField] float _TimeOfDayDebug = -1.0f;

    float _currentCycleDuration = 0f;

    Vector3 forward = Vector3.forward;

    void Start()
    {
        forward = transform.forward;
        _currentCycleDuration = _cycleStartRatio * _cycleDurationSeconds;

     //   gameObject.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    }

    void Update()
    {
        _currentCycleDuration += Time.deltaTime;
        UpdateTimeOfDay();
    }

    void UpdateTimeOfDay()
    {
        float lerpAmt = Mathf.Clamp((_currentCycleDuration - _cycleStartDelay) / _cycleDurationSeconds, 0.0f, 1.0f);
        if (_TimeOfDayDebug > -1.0f)
        {
            lerpAmt = _TimeOfDayDebug;
        }
        Shader.SetGlobalFloat("_TimeOfDay", lerpAmt);

        // Sun
        Color currentLightColor = Color.Lerp(_startSunColor, _EndSunColor, lerpAmt);
        _lightSun.intensity = currentLightColor.a;
        _lightSun.color = new Color(currentLightColor.r, currentLightColor.g, currentLightColor.b, 1.0f);

        // Tint
        Color currentTint = Color.Lerp(_startBackgroundTintColor, _endBackgroundTintColor, lerpAmt);
        Shader.SetGlobalColor("_BackgroundTintColor", currentTint);
    }

    private void OnValidate()
    {
        UpdateTimeOfDay();
    }
}
