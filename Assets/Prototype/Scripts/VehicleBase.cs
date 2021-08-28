using UnityEngine;

public abstract class VehicleBase : MonoBehaviour
{
    public VehicleInput Input;

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
