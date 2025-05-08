using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagneticPulling", menuName = "SO/Enemy/Pattern/MagneticPulling")]
public class MagneticPullingPatternSO : PatternDataSO
{
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return Vector3.Distance(executorTransform.position, targetTransform.position) > 5f;
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("MagneticPulling");
        priority = 0;
    }

    public override void UpdatePriority(Transform executorTransform, Transform targetTransform)
    {
        priority += 1;
    }
}
