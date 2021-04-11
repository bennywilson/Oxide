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
        
        var velocity = _body.velocity;
        var angularVelocity = _body.angularVelocity;
        var forward = _body.rotation * Vector3.forward;

        float speed = Vector3.Dot(velocity, forward);

        _body.velocity = velocity;
        _body.angularVelocity = velocity;
    }
}
