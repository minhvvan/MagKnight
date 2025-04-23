using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
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
        
        EnemyController.AddEnemy(this);
        
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

    private void OnAnimatorMove()
    {
        Vector3 rootDelta = Anim.deltaPosition;
        Vector3 scaledDelta = rootDelta * blackboard.abilitySystem.GetValue(AttributeType.MoveSpeed);
        
        Vector3 position = transform.position + scaledDelta;
        // Vector3 position = EnemyAnimator.rootPosition;
        position.y = Agent.nextPosition.y;
        
        Agent.nextPosition = position;
        transform.position = position;
        
        Vector3 dir = Agent.desiredVelocity;
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    public void OnDeath()
    {
        SetState(deadState);
    }

    public void OnStagger()
    {
        float maxRes = blackboard.abilitySystem.GetValue(AttributeType.MaxResistance);
        SetState(staggerState);
        
        // Set은 할 수 없습니다. 초기화에만 사용해주세요 : 이민준
        //blackboard.abilitySystem.SetValue(AttributeType.RES, maxRes);
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

    public async UniTask OnMeleeAttackHit(Transform playerTransform)
    {
        // Enemy 넉백 관련은 이 함수를 사용
        float desiredDistance = 1.5f;
        float pullSpeed = 20f;
        float moveTime = 0.3f;
        float duration = 0f;
        Vector3 targetPos = playerTransform.position + playerTransform.forward * desiredDistance;
        while (duration < moveTime)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, duration / moveTime);
            duration += Time.deltaTime;
            await UniTask.Yield();
        }
    }

    public void OnNext(HitInfo hitInfo)
    {
        float damage = -blackboard.abilitySystem.GetValue(AttributeType.Strength);
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.HP, damage);
        hitInfo.hit.collider.gameObject.GetComponent<AbilitySystem>().ApplyEffect(damageEffect);
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
    

    public void OnDestroy()
    {
        EnemyController.RemoveEnemy(this);
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
}
