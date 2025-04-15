using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour, IObserver<GameObject>
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    public EnemyBlackboard blackboard;
    public EnemyHitboxController hitboxController;
    
    // components
    public Animator Anim { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Collider MainCollider { get; private set; }
    public Rigidbody Rb { get; private set; }
    
    
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
        TestCode();
    }

    void OnEnable()
    {
        _stateMachine.ChangeState(spawnState);
    }
    public void Initialize()
    {
        blackboard = new EnemyBlackboard();
        blackboard.Initialize(_enemyDataSO, this);
        Anim = GetComponent<Animator>();
        // Anim.
        
        Agent = GetComponent<NavMeshAgent>();
        Agent.updatePosition = false;
        Agent.updateRotation = false;
        MainCollider = GetComponent<Collider>();
        Rb = GetComponent<Rigidbody>();
        
        // hitbox Controller
        hitboxController.Subscribe(this);
        
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
        Vector3 scaledDelta = rootDelta * blackboard.moveSpeed;
        
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
    
    #region targetDetecting

    public bool TargetInRange()
    {
        return Agent.remainingDistance <= blackboard.attackRange;
    }
    public bool TargetInRay()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float radius = 0.5f;
        return Physics.SphereCast(origin,
            radius,
            transform.forward,
            out _,
            blackboard.attackRange,
            blackboard.targetLayer
        );
    }
    #endregion

    public void OnHit()
    {
        // todo: player로부터 공격력, 강직도 감소율 받아오기
        float attackPower = 1f;
        float resistanceLoss = 1f;
        blackboard.currentHealth -= attackPower;
        blackboard.currentStaggerResistance -= resistanceLoss;

        if (blackboard.currentHealth <= 0)
        {
            SetState(deadState);
            return;
        }

        if (blackboard.currentStaggerResistance <= 0)
        {
            SetState(staggerState);
            blackboard.currentStaggerResistance = blackboard.staggerResistance;
        }
    }
    
    public void OnNext(GameObject value)
    {
        // value는 target
        // player의 피격 함수를 호출
        Debug.Log("플레이어 피격");
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
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
    
    private async UniTask TestCode()
    {
        await UniTask.WaitForSeconds(5f);
        int i = 0;
        while (i < 10)
        {
            Debug.Log("Hit");
            OnHit();
            i++;
            await UniTask.WaitForSeconds(2f);
        }
    }
    #endregion
}
