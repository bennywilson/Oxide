using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField]
    float BoostVelocity = 16;

    [SerializeField]
    float BoostAcceleration = 10;

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

        CarPhys.StartSpeedBoost(BoostVelocity, BoostAcceleration, BoostLength);
    }
}
