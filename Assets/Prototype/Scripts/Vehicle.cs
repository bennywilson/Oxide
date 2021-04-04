using UnityEngine;
using UnityEngine.InputSystem;

public class Vehicle : MonoBehaviour
{
    Rigidbody RB;
    Vector2 SteeringVector;
    float Gas;

    [SerializeField]
    float TurnRate;

    [SerializeField]
    float MaxSpeed;

    [SerializeField]
    float Acceleration = 1.0f;

    [SerializeField]
    float Friction = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        RB = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        RB = GetComponentInChildren<Rigidbody>();
        RB.hideFlags = HideFlags.NotEditable;
    }

    private void Update()
    {
        RB.drag = Friction;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 vel = transform.forward;
        vel *= MaxSpeed * Gas;
        RB.velocity += transform.forward * Gas * Acceleration;

        if (Mathf.Abs(SteeringVector.x) > 0.1f)
        {
            transform.rotation *= Quaternion.AngleAxis(SteeringVector.x, transform.up);
        }
    }

    private void LateUpdate()
    {

       // this.transform.Rotate(0.0f, SteeringVector.x * TurnRate, 0.0f, Space.World);
    }

    public void OnMoveInput(InputValue Value)
    {
        SteeringVector = Value.Get<Vector2>();
    }

    public void OnGas(InputValue Value)
    {
    //    RB.lin
        Gas = Value.Get<float>();
    }
}
