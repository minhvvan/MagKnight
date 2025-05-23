using UnityEngine;
using UnityEngine.AI;

public class RangedNormalAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    // private Collider _targetCollider;
    
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer = 0f;

    public RangedNormalAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
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
        
        if (TargetInRayAndVisible())
        {
            _enemy.SetState(_enemy.actionState);
            return;
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
        _destinationTimer = destinationUpdateInterval;
        // _enemy.Anim.SetBool("Trace", false);
    }
    
    public bool TargetInRayAndVisible()
    {
        // Ranged Enemy를 위한 탐색
        if (_blackboard.target == null) return false;

        Vector3 origin = _blackboard.muzzleTransform.position; // 총구 위치 또는 눈 위치
        Vector3 targetPos = _blackboard.target.transform.position + Vector3.up * 0.5f;

        float distance = Vector3.Distance(origin, targetPos);
        if (distance > _blackboard.attackRange)
            return false;

        // 시야 차단 Raycast (장애물 검사)
        Vector3 direction = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, _blackboard.attackRange, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == _blackboard.target)
            {
                return true; // 플레이어까지 막힘 없이 도달
            }
            else
            {
                // 중간에 막힌 것
                Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
                return false;
            }
        }

        return false;
    }
}
