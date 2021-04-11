[System.Serializable]
public struct CarPhysicsSettings
{
    // Single-gear at the moment
    public float Acceleration;
    public float TopSpeed;
    public float TurnAngleLowSpeed;
    public float TurnAngleTopSpeed;
    public float SteeringSpeed;
    public float DriftFriction;
    public float CarRotationSpeedPerUnitOfOffset;
}
