using UnityEngine;

public struct MagnetActionInertiaStateData : IStateData
{
    public Vector3 finalVelocity;

    public MagnetActionInertiaStateData(Vector3 finalVelocity)
    {
        this.finalVelocity = finalVelocity;
    }
}