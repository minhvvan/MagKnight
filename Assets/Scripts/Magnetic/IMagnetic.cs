using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMagnetic
{
    void Initialize();
    void SetPhysic();
    
}

public enum MagneticType
{
    N,
    S
}