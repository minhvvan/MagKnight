using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Moon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AbilitySystem))]
[RequireComponent(typeof(EnemyBlackboard))]
public class Enemy : MagneticObject, IObserver<HitInfo>
{
    // components
    public Animator Anim { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Collider MainCollider { get; private set; }
    public Rigidbody Rb { get; private set; }
    public AbilitySystem EnemyAbilitySystem { get; private set; }
    public HitDetector HitHandler { get; private set; } // Melee type enemy만 enemy한테 붙어있음
    public EnemyBlackboard blackboard;
    public PatternController patternController;
    public HpBarController hpBarController;

    private AnimatorStateInfo _currentAnimStateInfo;

    public Action<Enemy> OnDead;
    
    // stateMachine
    private StateMachine _stateMachine;
    
    [NonSerialized] public EnemyStateSpawn spawnState;
    [NonSerialized] public EnemyStateAI aiState;
    [NonSerialized] public EnemyStateAction actionState;
    [NonSerialized] public EnemyStateStagger staggerState;
    [NonSerialized] public EnemyStateDead deadState;

    void Awake()
    {
        Initialize();
        InitializeMagnetic();
        // TestCode();
    }

    void OnEnable()
    {
        _stateMachine.ChangeState(spawnState);
    }
    private void Initialize()
    {
        Anim = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        MainCollider = GetComponent<Collider>();
        Rb = GetComponent<Rigidbody>();
        EnemyAbilitySystem = GetComponent<AbilitySystem>();

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        
        // hitbox Controller
        HitDetector hitHandler;
        if (TryGetComponent<HitDetector>(out hitHandler))
        {
            HitHandler = hitHandler;
            HitHandler.Subscribe(this);
        }
        
        InitializeState();
    }

    private void InitializeState()
    {
        _stateMachine = new StateMachine();

        spawnState = new EnemyStateSpawn(this);
        aiState = new EnemyStateAI(this);
        actionState = new EnemyStateAction(this);
        staggerState = new EnemyStateStagger(this);
        deadState = new EnemyStateDead(this);

        _stateMachine.ChangeState(spawnState);
    }

    void Update()
    {
        _stateMachine.Update();
        // var state = Anim.GetCurrentAnimatorStateInfo(0);
        // Debug.Log("현재 상태: " + state.fullPathHash);
    }

    public void SetState(BaseState<Enemy> newState)
    {
        _stateMachine.ChangeState(newState);
    }
    
    public bool IsCurrentAnimFinished(string animName)
    {
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animName) && info.normalizedTime >= 1f;
    }

    public bool IsCurrentAnim(string animName)
    {
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animName);
    }

    private void FixedUpdate()
    { 
        _currentAnimStateInfo = Anim.GetCurrentAnimatorStateInfo(0);
        var enemies = Physics.OverlapSphere(transform.position, 0.5f, 1 << LayerMask.NameToLayer("Enemy"));

        ApplySoftCollision(enemies);
    }

    private void OnAnimatorMove()
    {
        if (_currentAnimStateInfo.IsName("Trace"))
        {
            Vector3 velocity = Agent.desiredVelocity.normalized * Time.deltaTime * 
                               blackboard.abilitySystem.GetValue(AttributeType.MoveSpeed);
            
            Agent.nextPosition += velocity;
            transform.position += velocity;
        }
        
        // Vector3 rootDelta = Anim.deltaPosition;
        // Vector3 scaledDelta = rootDelta * blackboard.abilitySystem.GetValue(AttributeType.MoveSpeed);
        //
        // Vector3 position = transform.position + scaledDelta;
        // position.y = Agent.nextPosition.y;
        // Agent.nextPosition = position;
        // transform.position = position;
        
        Vector3 dir = Agent.desiredVelocity;
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    public void OnDeath()
    {
        if(blackboard.isDead) return;

        blackboard.isDead = true;
        SetState(deadState);
        OnDead?.Invoke(this);
        
        //죽으면 일정 확률로 힐팩 생성
        if(ItemManager.Instance.CheckProbability(ItemCategory.HealthPack, ItemRarity.Common))
        {
            ItemManager.Instance.CreateItem(ItemCategory.HealthPack, ItemRarity.Common, 
                MainCollider.bounds.center + Vector3.up, Quaternion.identity);
        }
    }

    public void OnStagger()
    {
        if (blackboard.isDead) return;
        SetState(staggerState);
    }
    
    public void OnPhaseChange(int phase)
    {
        if (blackboard.aiType == EnemyAIType.Boss)
        {
            Anim.SetTrigger("PhaseChange");
            blackboard.phase = phase;
            patternController.PhaseChange(phase);
        }
    }

    public void OnHit(ExtraData extraData)
    {
        // 붉은색으로 변환, enemy 위치 보정
        
        if(blackboard.isDead) return;
        
        if (blackboard.onHitCancellation != null)
        {
            blackboard.onHitCancellation.Cancel();
            blackboard.onHitCancellation.Dispose();
        }
        blackboard.onHitCancellation = new CancellationTokenSource();
        
        OnHitHelper(extraData).Forget();
    }

    private async UniTask OnHitHelper(ExtraData extraData)
    {
        CancellationToken token = blackboard.onHitCancellation.Token;
        
        blackboard.enemyRenderer.material.SetTexture("_BaseColor", blackboard.onHitTexture);
        
        // Enemy 넉백 관련은 이 함수를 사용
        float desiredDistance = extraData.weaponRange;
        float enemyPositionCorrection = 0.5f;
        float pullSpeed = 20f;
        float moveTime = 0.2f;
        float duration = 0f;
        
        Vector3 forward = extraData.sourceTransform.forward;
        Vector3 right = extraData.sourceTransform.right;
        Vector3 targetPos = transform.position + forward;
        if ((targetPos - extraData.sourceTransform.position).magnitude > desiredDistance)
        {
            Vector3 dir = transform.position - extraData.sourceTransform.position;
            float rightSize = Vector3.Dot(dir, right);
            if (Mathf.Abs(rightSize) > desiredDistance)
            {
                targetPos = extraData.sourceTransform.position + dir.normalized * desiredDistance;
            }
            else
            {
                float delta = Mathf.Sqrt(desiredDistance * desiredDistance - rightSize * rightSize) 
                              - Vector3.Dot(dir, forward);
                targetPos = transform.position + forward * delta;
            }
        }

        try
        {
            while (duration < moveTime)
            {
                Vector3 newPos = Vector3.Lerp(transform.position, targetPos, duration / moveTime);
                transform.position = newPos;
                Agent.nextPosition = newPos;
                duration += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
        catch (OperationCanceledException){}
        finally
        {
            blackboard.enemyRenderer.material.SetTexture("_BaseColor", blackboard.baseColorTexture);
        }
    }
    
    public void OnDamaged()
    {
        // 체력바 감소
        hpBarController.SetHP(blackboard.abilitySystem.GetValue(AttributeType.HP)/blackboard.abilitySystem.GetValue(AttributeType.MaxHP));
    }

    public void OnNext(HitInfo hitInfo)
    {
        GiveDamageEffect(hitInfo);
    }

    public void GiveDamageEffect(HitInfo hitInfo)
    {
        float damage = blackboard.abilitySystem.GetValue(AttributeType.Strength);
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, damage);
        GameplayEffect impulseEffect = new GameplayEffect(EffectType.Instant, AttributeType.Impulse, 30);
        damageEffect.extraData.sourceTransform = transform;
        impulseEffect.extraData.sourceTransform = transform;
        hitInfo.collider.gameObject.GetComponent<AbilitySystem>().ApplyEffect(damageEffect);
        hitInfo.collider.gameObject.GetComponent<AbilitySystem>().ApplyEffect(impulseEffect);
    }

    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }
    
    public void MeleeAttackStart(int throwing = 0)
    {
        HitHandler.StartDetection();
    }

    public void MeleeAttackEnd()
    {
        HitHandler.StopDetection();
    }

    public void PatternAttackStart()
    {
        patternController.AttackStart();
    }

    public void PatternAttackEnd()
    {
        patternController.AttackEnd();
    }

    //특정 대상에게 극성 상관없이 정해진 동작만을 수행하게도 가능.
    public override async UniTask OnMagneticInteract(MagneticObject target)
    {
        //ex. Enemy에겐 사용 시 무조건 돌진한다.
        //await magnetApproach.Execute(this, target);
        await magnetDashAttackAction.Execute(this, target);
    }

    void ApplySoftCollision(Collider[] colliders)
    {
        Vector3 correction = Vector3.zero;
        foreach (var other in colliders)
        {
            if (other.gameObject == gameObject) continue;

            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;
            float minDist = 0.5f; // 적당한 간격
            
            if (dist < 0.001f) // 완벽하게 겹칠 경우에는 diff가 Vector3.zero가 되므로 예외처리
                correction += new Vector3(1, 0, 0);
            
            else if (dist < minDist && dist > 0.001f)
            {
                float pushStrength = (minDist - dist) / minDist;
                correction += diff.normalized * pushStrength;
            }
        }

        Vector3 newPos = transform.position + correction * Time.deltaTime * 2f;
        newPos.y = Agent.nextPosition.y;

        transform.position = newPos;
        Agent.nextPosition = newPos;
    }
    #region debugging
    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(transform.position, blackboard.attackRange);
        //
        // // Agent 목적지 시각화
        // if (Agent != null && Agent.hasPath)
        // {
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(Agent.destination, 0.5f);
        //     Gizmos.DrawLine(Agent.destination, Agent.destination);
        // }
        // Gizmos.color = Color.blue;
        // Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * blackboard.attackRange);
    }
    
    #endregion
    
    public bool IsAvailableTarget()
    {
        GameObject target = blackboard.target;
        if (target == null) return false;
        if (target.TryGetComponent(out PlayerController player))
        {
            return !player.IsInvisible;
        }
        return false;
    }
}
