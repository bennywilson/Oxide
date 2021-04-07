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
    float TurnFriction = 0.0f;

    [SerializeField]
    private GameObject Driver;

    // Start is called before the first frame update
    void Start()
    {
        RB = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        RB = GetComponentInChildren<Rigidbody>();
        RB.hideFlags = HideFlags.NotEditable;
    }

    private void Update()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Steer
        float FacingVelDot = Vector3.Dot(transform.forward, RB.velocity);
        float SteerAmt = SteeringVector.x * FacingVelDot;
        if (Mathf.Abs(SteerAmt) > 0.1f)
        {
            transform.rotation *= Quaternion.AngleAxis(SteeringVector.x, transform.up);
        }

        // Gas
        Vector3 Dir2D = transform.forward;
        Dir2D.y = 0;
        Dir2D.Normalize();

        if (Gas > 0.0f)
        {
            RB.AddForce(transform.forward * Gas * Acceleration, ForceMode.Acceleration);

            Vector3 curVec = new Vector3(RB.velocity.x, 0.0f, RB.velocity.z);
            if (curVec.magnitude > MaxSpeed)
            {
                curVec = Dir2D * MaxSpeed;
            }
            curVec.y = RB.velocity.y;
            RB.velocity = curVec;
        }

        // Friction
        float TotalFriction = TurnFriction * Mathf.Abs(SteeringVector.x);
        Vector3 FrictionVec = RB.velocity;
        FrictionVec.y = 0;
        FrictionVec.Normalize();

        FrictionVec *= TotalFriction;
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
    public void OnPrrr(InputValue Value)
    {
        Animation anim = Driver.GetComponentInChildren<Animation>();
        anim.enabled = true;
        anim.Play();
    }
}