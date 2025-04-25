using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeNormalAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer = 0f;

    public MeleeNormalAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
    }
    
    public void OnEnter()
    {
        _enemy.Agent.SetDestination(_blackboard.target.transform.position);
        if (_enemy.Agent.pathPending == false && _enemy.Agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            // 갈 수 없는 곳이거나, 경로 계산 실패!
            _enemy.Anim.SetBool("Trace", true);
        }
    }

    public void OnUpdate()
    {
        if (TargetInRay())
        {
            _enemy.SetState(_enemy.actionState);
            return;
        }
        else
        {
            _destinationTimer += Time.deltaTime;
            if (_destinationTimer >= destinationUpdateInterval)
            {
                _enemy.Agent.SetDestination(_blackboard.target.transform.position);
                _destinationTimer = 0f;
            }
        }

        if (_enemy.Agent.pathPending == false)
        {
            if (_enemy.Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                _enemy.Anim.SetBool("Trace", true);
            }
            else
            {
                _enemy.Anim.SetBool("Trace", false);
            }
        } 
    }

    public void OnExit()
    {
        _enemy.Anim.SetBool("Trace", false);
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
}
