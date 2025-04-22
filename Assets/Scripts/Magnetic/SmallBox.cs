using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SmallBox : MagneticObject
{
    private void Awake()
    {
        InitializeMagnetic();
    }
    
    public override async UniTask OnMagneticInteract(MagneticObject target)
    {
        if (target.magneticType != magneticType)
        {
            await magnetApproach.Execute(target, this);
        }
        else if (target.magneticType == magneticType)
        {
            await magnetSeparation.Execute(target, this);
        }
    }
}
