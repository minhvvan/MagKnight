using UnityEngine;

public struct MagnetActionApproachToTransformStateData : IStateData
{
    public Transform toTransform;
    
    public MagnetActionApproachToTransformStateData(Transform toTransform)
    {
        this.toTransform = toTransform;
    }
}