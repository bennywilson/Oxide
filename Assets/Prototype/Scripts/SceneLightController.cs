using UnityEngine;

/// <summary>
/// Day/night cycle controller
/// </summary>
public class SceneLightController : MonoBehaviour
{
    [SerializeField] float _cycleDurationSeconds = 30f;
    [SerializeField] float _cycleStartRatio = 0.5f;

    [SerializeField] Light _lightSun = null;
    [SerializeField] Light _lightMoon = null;

    [SerializeField] float _maxSunIntensity = 1f;
    [SerializeField] float _maxMoonIntensity = 0.5f;

    float _currentCycleDuration = 0f;

    Vector3 forward = Vector3.forward;

    void Start()
    {
        forward = transform.forward;
        _currentCycleDuration = _cycleStartRatio * _cycleDurationSeconds;
    }

    void Update()
    {
        _currentCycleDuration += Time.deltaTime;
        _currentCycleDuration %= _cycleDurationSeconds;

        float cycleSine = Mathf.Sin(Mathf.PI * 2 * _currentCycleDuration / _cycleDurationSeconds);

        if (_lightSun != null)
        {
            float sunItensity = cycleSine * _maxSunIntensity;
            _lightSun.intensity = Mathf.Max(0, sunItensity);
            _lightSun.enabled = sunItensity > 0;
        }

        if (_lightMoon != null)
        {
            float moonIntensity = -cycleSine * _maxMoonIntensity;
            _lightMoon.intensity = Mathf.Max(0, moonIntensity);
            _lightMoon.enabled = moonIntensity > 0;
        }

        transform.rotation = Quaternion.AngleAxis(360f * _currentCycleDuration / _cycleDurationSeconds, forward);
    }
}
