using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IMagnetic
{
    void InitializeMagnetic();
    void SetPhysic();
    UniTask OnMagneticInteract(MagneticObject target);
}

public enum MagneticType
{
    N,
    S
}