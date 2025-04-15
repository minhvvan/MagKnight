using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagneticObject : MonoBehaviour, IMagnetic
{
    public MagneticType magneticType; //오브젝트의 극 (N,S)
    public bool isStructure; //움직이지 않는 구조물인가?
    public float objectMass; //오브젝트 중량
    public MagneticObjectSO magneticObjectSO;
    public Rigidbody rigidbody;

    public virtual void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        SetPhysic();
    }

    public virtual void SetPhysic()
    {
        magneticType = magneticObjectSO.magneticType;
        isStructure = magneticObjectSO.isStructure;
        objectMass = magneticObjectSO.objectMass;

        if (isStructure)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
    }

    public bool GetIsStructure()
    {
        return isStructure;
    }

    public void SwitchMagneticType()
    {
        if(magneticType == MagneticType.N) magneticType = MagneticType.S;
        else if(magneticType == MagneticType.S) magneticType = MagneticType.N;
    }
}
