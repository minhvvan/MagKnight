using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagneticPulling", menuName = "SO/Enemy/Pattern/MagneticPulling")]
public class MagneticPullingPatternSO : PatternDataSO
{
    RaycastHit[] hits = new RaycastHit[1];
    
    public override bool CanUse(Transform executorTransform, Transform targetTransform)
    {
        return true;
    }

    public override void Execute(Animator animator)
    {
        animator.SetTrigger("MagneticPulling");
    }
}
