using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossPattern", menuName = "SO/Enemy/Pattern/SpawnBomb")]
public class SpawnBombPatternSO : PatternDataSO
{
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return TargetInRange(executorTransform, targetTransform);
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("SpawnBomb");
        priority = 0;
    }

    public override void UpdatePriority(Transform executorTransform, Transform targetTransform)
    {
        priority += 1;
    }

    private bool TargetInRange(Transform executorTransform, Transform targetTransform)
    {
        return Vector3.Distance(executorTransform.position, targetTransform.position) < range;
    }
}
