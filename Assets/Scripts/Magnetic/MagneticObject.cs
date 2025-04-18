using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

public class MagneticObject : MonoBehaviour, IMagnetic
{
    public MagneticType magneticType; //오브젝트의 극 (N,S)
    public bool isStructure; //움직이지 않는 구조물인가?
    public float objectMass; //오브젝트 중량
    public MagneticObjectSO magneticObjectSO;
    public Rigidbody rb; //

    public virtual void Initialize()
    {
        TryGetComponent(out rb);
        SetPhysic();
    }

    public virtual async UniTask LoadMagneticObjectSO()
    {
        //magneticObjectSO = await DataManager.Instance.LoadDataAsync<>() 
    }

    public virtual void SetPhysic()
    {
        magneticType = magneticObjectSO.magneticType = magneticObjectSO != null ? magneticObjectSO.magneticType : MagneticType.N;
        isStructure = magneticObjectSO.isStructure = magneticObjectSO != null && magneticObjectSO.isStructure;
        objectMass = magneticObjectSO.objectMass = magneticObjectSO != null ? magneticObjectSO.objectMass : 1;

        if (isStructure && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // public bool GetIsStructure()
    public virtual bool GetIsStructure()
    {
        return isStructure;
    }

    public virtual MagneticType GetMagneticType()
    {
        return magneticType;
    }

    public virtual void SwitchMagneticType(MagneticType? type = null)
    {
        if (type.HasValue)
        {
            magneticType = type.Value;
            return;
        }
        
        if(magneticType == MagneticType.N) magneticType = MagneticType.S;
        else if(magneticType == MagneticType.S) magneticType = MagneticType.N;
    }
}
