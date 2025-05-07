using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Effector))]
public class MagneticObject : MonoBehaviour, IMagnetic
{
    public MagneticType magneticType; //오브젝트의 극 (N,S)
    public bool isStructure; //움직이지 않는 구조물인가?
    public float objectMass; //오브젝트 중량
    public MagneticObjectSO magneticObjectSO;
    public Rigidbody rb; //

    public IMagneticInteractCommand magnetApproach;
    public IMagneticInteractCommand magnetSeparation;
    public IMagneticInteractCommand magnetDashAttackAction;
    public IMagneticInteractCommand magnetDashJumpAction;
    public IMagneticInteractCommand magnetSwingAction;
    
    public Effector Effector { get; protected set; }

    protected virtual void Awake()
    {
        Effector = GetComponent<Effector>();
    }

    public virtual void InitializeMagnetic()
    {
        //기본 데이터 세팅
        TryGetComponent(out rb);
        SetPhysic();
        
        //자기력 상호작용
        SetMagneticInteract();
        
        //Highlight Binding
        BindMagneticHighlight();
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
    
    //새로 추가된 자기력 관련 상호작용을 이곳에서 캐싱합니다.
    public virtual void SetMagneticInteract()
    {
        magnetApproach = MagneticInteractFactory.GetInteract<OnApproach>();
        magnetSeparation = MagneticInteractFactory.GetInteract<OnSeparation>();
        magnetDashAttackAction = MagneticInteractFactory.GetInteract<MagnetDashAttackAction>();
        magnetDashJumpAction = MagneticInteractFactory.GetInteract<MagnetDashJumpAction>();
        magnetSwingAction = MagneticInteractFactory.GetInteract<MagnetSwingAction>();
    }
    
    private void BindMagneticHighlight()
    {
        //MagneticHighlightController 찾아서 renderer묶어주기
        var magneticHighlighter = FindObjectOfType<MagneticHighlightController>();
        magneticHighlighter.BindRenderer(gameObject, magneticType);
    }

    public virtual async UniTask OnMagneticInteract(MagneticObject target)
    {
        //끌려오기 날아가기 등 다양한 액션을 override하여 사용.
        
        //디폴트

        if (target.magneticType != magneticType)
        {
            await magnetApproach.Execute(target, this);
        }
        else if (target.magneticType == magneticType)
        {
            await magnetSeparation.Execute(target, this);
        }
    }

    public virtual void MagneticCoolDown()
    {
        
    }

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
