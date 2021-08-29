using UnityEngine;

public abstract class VehicleBase : MonoBehaviour
{
    public VehicleInput Input;

    [SerializeField]
    protected GameObject Driver;

    [SerializeField]
    protected GameObject Passenger;

    float _distanceAlongSpline;
    public float DistanceAlongSpline
    {
        get
        {
            return _distanceAlongSpline;
        }
        protected set
        {
            _distanceAlongSpline = value;
        }
    }
}
