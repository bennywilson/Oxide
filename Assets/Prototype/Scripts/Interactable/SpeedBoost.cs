using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField]
    float BoostVelocityMultiplier = 100;

    [SerializeField]
    float BoostAccelerationMultiplier = 10;

    [SerializeField]
    float BoostLength = 3.0f;
    
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == null)
        {
            return;
        }

        CarPhysicsObject CarPhys = collision.gameObject.GetComponent(typeof(CarPhysicsObject)) as CarPhysicsObject;
        if (CarPhys == null)
        {
            return;
        }

        CarPhys.StartSpeedBoost(BoostVelocityMultiplier, BoostAccelerationMultiplier, BoostLength);
    }
}
