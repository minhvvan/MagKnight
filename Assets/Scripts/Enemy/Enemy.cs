using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour, IObserver<GameObject>
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    public EnemyBlackboard blackboard;
    public EnemyAIController aiController;
    public EnemyHitboxController hitboxController;
    
    public Animator EnemyAnimator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    
    
    // stateMachine
    private StateMachine _stateMachine;
    
    [NonSerialized] public EnemyStateSpawn spawnState;
    [NonSerialized] public EnemyStateAI aiState;
    [NonSerialized] public EnemyStateAction actionState;
    [NonSerialized] public EnemyStateHit hitState;
    [NonSerialized] public EnemyStateDead deadState;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        _stateMachine.ChangeState(spawnState);
    }
    public void Initialize()
    {
        blackboard = new EnemyBlackboard();
        blackboard.Initialize(_enemyDataSO, this);
        EnemyAnimator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Agent.updatePosition = false;
        Agent.updateRotation = false;

        // ai Controller
        aiController = new EnemyAIController();
        
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
        hitState = new EnemyStateHit(this);
        deadState = new EnemyStateDead(this);

        _stateMachine.ChangeState(spawnState);
    }

    void Update()
    {
        _stateMachine.Update();
    }

    public void SetState(BaseState<Enemy> newState)
    {
        _stateMachine.ChangeState(newState);
    }
    
    public bool IsCurrentAnimFinished(string animName)
    {
        AnimatorStateInfo info = EnemyAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animName) && info.normalizedTime >= 1f;
    }

    private void OnAnimatorMove()
    {
        Vector3 position = EnemyAnimator.rootPosition;
        
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
        return Physics.Raycast(origin,
            transform.forward,
            out _,
            blackboard.attackRange,
            blackboard.targetLayer
        );
    }
    #endregion
    
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
}
