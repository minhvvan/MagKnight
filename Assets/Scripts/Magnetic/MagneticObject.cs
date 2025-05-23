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
    public bool isMagneticHighlight = true;
    
    //UI에서 표시되는 포인트(상호작용 시작 지점)
    public Transform magneticPoint;
    //실제 물리 연산이 일어날 지점
    public Transform magneticTargetPoint;

    public IMagneticInteractCommand magnetApproach;
    public IMagneticInteractCommand magnetSeparation;
    public IMagneticInteractCommand magnetDashAttackAction;
    public IMagneticInteractCommand magnetDashJumpAction;
    public IMagneticInteractCommand magnetSwingAction;
    public IMagneticInteractCommand magnetPlatePullAction;
    public IMagneticInteractCommand magnetPlateThrowAction;
    public MagnetPlate magnetPlate;
    public bool isGetPlate;
    
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
    }

    protected virtual void OnEnable()
    {
        //Highlight Binding
        if (isMagneticHighlight)
        {
            BindMagneticHighlight();
        }
    }

    private void OnDisable()
    {
        //Highlight Unbinding
        if (isMagneticHighlight)
        {
            UnbindMagneticHighlight();
        }
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
        magnetPlatePullAction = MagneticInteractFactory.GetInteract<MagnetPlatePullAction>();
        magnetPlateThrowAction = MagneticInteractFactory.GetInteract<MagnetPlateThrowAction>();
    }
    
    private void BindMagneticHighlight()
    {
        //MagneticHighlightController 찾아서 renderer묶어주기
        var magneticHighlighter = FindObjectOfType<MagneticHighlightController>();
        if (magneticHighlighter)
        {
            magneticHighlighter.BindRenderer(gameObject, magneticType);
        }
    }

    private void UnbindMagneticHighlight()
    {
        var magneticHighlighter = FindObjectOfType<MagneticHighlightController>();
        if (magneticHighlighter)
        {
            magneticHighlighter.UnbindRenderer(gameObject, magneticType);
        }
    }
    
    public async UniTask RunMagneticInteract(MagneticObject target, bool isHoldPlate = false)
    {
        //plate를 hold중인 상태라면 선택한 타겟에게 던지는 액션을 우선 구사한다.
        if (isHoldPlate)
        {
            //이 구간의 target은 Player
            if(target.magnetPlate != null) target.magnetPlate.isHold = false;
            await magnetPlateThrowAction.Execute(target, this);
            target.magnetPlate = null;
            return;
        }
        
        await OnMagneticInteract(target);
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

    public bool GetMagnetPlate()
    {
        return magnetPlate != null;
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

    protected void OnDestroy()
    {
        if (isMagneticHighlight)
        {
            UnbindMagneticHighlight();
        }
    }
}
