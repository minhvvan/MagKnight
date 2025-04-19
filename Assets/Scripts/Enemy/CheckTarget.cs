using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyTargeting
{
    public bool TargetInRay(Transform transform, float range, LayerMask layerMask);
}

public class EnemyMeleeTargeting : IEnemyTargeting
{
    public bool TargetInRay(Transform transform, float range, LayerMask layerMask)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float radius = 0.5f;
        return Physics.SphereCast(origin,
            radius,
            transform.forward,
            out _,
            range,
            layerMask
        );
    }
}

public class EnemyRangedTargeting : IEnemyTargeting
{
    public bool TargetInRay(Transform transform, float range, LayerMask layerMask)
    {
        // 원거리 적은 enemy가
        
        
        return default;
    }
}
