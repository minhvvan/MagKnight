using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagneticShield", menuName = "SO/Enemy/Pattern/MagneticShield")]
public class MagneticShieldPatternSO : PatternDataSO
{
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return true;
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("MagneticShield");
        priority = 0;
    }

    public override void UpdatePriority(Transform executorTransform, Transform targetTransform)
    {
        priority += 1;
    }
}
