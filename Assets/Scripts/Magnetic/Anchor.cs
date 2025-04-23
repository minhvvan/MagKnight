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
        await magnetApproach.Execute(this, target);
    }
}
