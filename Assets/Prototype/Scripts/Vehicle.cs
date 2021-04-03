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

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vel = transform.forward;
        vel *= MaxSpeed * Gas;
        RB.velocity = vel;
    }

    private void LateUpdate()
    {

        this.transform.Rotate(0.0f, SteeringVector.x * TurnRate, 0.0f, Space.World);
    }

    public void OnMoveInput(InputValue Value)
    {
        SteeringVector = Value.Get<Vector2>();

        Debug.Log(" -> " + SteeringVector);//
    }

    public void OnGas(InputValue Value)
    {
        Gas = Value.Get<float>();
    }
}
