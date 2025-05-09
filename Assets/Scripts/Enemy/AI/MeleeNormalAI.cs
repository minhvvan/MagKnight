using UnityEngine;
using UnityEngine.AI;

public class MeleeNormalAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer;

    public MeleeNormalAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
        destinationUpdateInterval = 0.1f;
    }
    
    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        if (TargetInRay())
        {
            _enemy.SetState(_enemy.actionState);
            return;
        }

        _destinationTimer += Time.deltaTime;
        if (_destinationTimer >= destinationUpdateInterval)
        {
            if(_enemy.IsAvailableTarget())
            {
                _enemy.Agent.SetDestination(_blackboard.target.transform.position);
            }

            _destinationTimer = 0f;
        }
        
        if (!_enemy.Agent.pathPending)
        {
            if (_enemy.Agent.hasPath && _enemy.Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                _enemy.SetAnimBool("Trace", true);
            }
            else
            {
                _enemy.SetAnimBool("Trace", false);
            }
        } 
    }

    public void OnExit()
    {
        _enemy.SetAnimBool("Trace", false);
    }
    
    public bool TargetInRay()
    {
        Transform enemyTransform = _enemy.transform;
        // Melee Enemy를 위한 탐색
        if ((enemyTransform.position - _blackboard.target.transform.position).magnitude < 1f) return true; // 너무 가까울때
        
        Vector3 origin = enemyTransform.position + Vector3.up * 0.5f;
        float radius = 0.5f;
        return Physics.SphereCast(origin,
            radius,
            enemyTransform.forward,
            out _,
            _blackboard.attackRange,
            _blackboard.targetLayer
        );
    }

    // public void SetDestination(Vector3 position)
    // {
    //     if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
    //     {
    //         // 1만큼 떨어진 곳에 navmesh가 있을경우
    //         _enemy.Agent.SetDestination(hit.position);
    //     }
    //     else
    //     {
    //         _enemy.Agent.ResetPath();
    //     }
    //     
    // }
}
