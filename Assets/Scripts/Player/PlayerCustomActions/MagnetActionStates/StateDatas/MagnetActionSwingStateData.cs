using UnityEngine;

public struct MagnetActionSwingStateData : IStateData
{
    public Transform anchorTransform;
    public float ropeLength;
    
    public MagnetActionSwingStateData(Transform anchorTransform, float ropeLength)
    {
        this.anchorTransform = anchorTransform;
        this.ropeLength = ropeLength;
    }
}