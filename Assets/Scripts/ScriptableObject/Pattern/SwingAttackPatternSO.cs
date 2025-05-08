using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossPattern", menuName = "SO/Enemy/Pattern/SwingAttack")]
public class SwingAttackPatternSO : PatternDataSO
{
    RaycastHit[] hits = new RaycastHit[1];
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return TargetInRange(executorTransform, targetTransform);
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("SwingAttack");
        priority = 0;
    }

    public override void UpdatePriority(Transform executorTransform, Transform targetTransform)
    {
        priority += 1;
    }

    private bool TargetInRange(Transform executorTransform, Transform targetTransform)
    {
        // Melee Enemy를 위한 탐색
        if ((executorTransform.position - targetTransform.position).magnitude < 1f) return true; // 너무 가까울때
        LayerMask targetLayer = 1 << targetTransform.gameObject.layer;
        Vector3 origin = executorTransform.position + Vector3.up * 0.5f;
        
        float radius = 0.5f;
        int hitCount = Physics.SphereCastNonAlloc(origin,
            radius,
            executorTransform.forward,
            hits,
            range,
            targetLayer
        );
        return hitCount > 0;
    }
}
