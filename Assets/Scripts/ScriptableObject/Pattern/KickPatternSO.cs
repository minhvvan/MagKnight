using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Kick", menuName = "SO/Enemy/Pattern/Kick")]
public class KickPatternSO : PatternDataSO
{
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return !isCooldown && TargetInRange(executorTransform, targetTransform);
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("Kick");
    }

    private bool TargetInRange(Transform executorTransform, Transform targetTransform)
    {
        Transform enemyTransform = executorTransform;
        // Melee Enemy를 위한 탐색
        if ((enemyTransform.position - targetTransform.position).magnitude < 1f) return true; // 너무 가까울때
        
        LayerMask targetLayer = targetTransform.gameObject.layer;
        
        Vector3 origin = targetTransform.TryGetComponent<Collider>(out Collider collider) ? 
            collider.bounds.center : enemyTransform.position + Vector3.up * 0.5f;
        
        float radius = 0.5f;
        return Physics.SphereCast(origin,
            radius,
            enemyTransform.forward,
            out _,
            range,
            targetLayer
        );
    }
}
