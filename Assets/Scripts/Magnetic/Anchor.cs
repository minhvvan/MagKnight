using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Anchor : MagneticObject
{
    private void Awake()
    {
        InitializeMagnetic();
    }
    
    public override async UniTask OnMagneticInteract(MagneticObject target)
    {
        if (target.magneticType != magneticType)
        {
            await magnetDashJumpAction.Execute(this, target);
        }
        else if (target.magneticType == magneticType)
        {
            await magnetSwingAction.Execute(this, target);
        }
    }
}
