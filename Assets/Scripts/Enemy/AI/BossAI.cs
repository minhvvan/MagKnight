using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 1. 매 프레임 시전 조건을 만족하는 패턴이 있는지 체크
/// 2. 조건을 만족하는 패턴이 있을 경우 해당 Action을 시전
/// 3. 조건 만족 패턴이 여러개일 경우, 우선순위가 높은 패턴을 시전
/// 4. 패턴 시전 후에는 1번부터 반복
/// </summary>
public class BossAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private PatternController _patternController;
    
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer;

    public BossAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
        _destinationTimer = destinationUpdateInterval;
        _patternController = _enemy.GetComponent<PatternController>();
    }
    
    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        _destinationTimer += Time.deltaTime;
        if (_destinationTimer >= destinationUpdateInterval)
        {
            if(_enemy.IsAvailableTarget())
            {
                _enemy.Agent.SetDestination(_blackboard.target.transform.position);
            }

            _destinationTimer = 0f;
        }
        
        PatternDataSO patternDataSO = _patternController.GetAvailablePattern(_enemy.transform, _blackboard.target.transform);
        if(patternDataSO != null)
        {
            _enemy.SetState(_enemy.actionState);
            return;
        }
        
        if (!_enemy.Agent.pathPending)
        {
            if (_enemy.Agent.hasPath && _enemy.Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                _enemy.SetAnimBool("Trace", true);
                // _enemy.Anim.SetBool("Trace", true);
            }
            else
            {
                _enemy.SetAnimBool("Trace", false);
                // _enemy.Anim.SetBool("Trace", false);
            }
        } 
    }

    public void OnExit()
    {
        _enemy.SetAnimBool("Trace", false);
        _destinationTimer = destinationUpdateInterval;
        // _enemy.Anim.SetBool("Trace", false);
    }
}
