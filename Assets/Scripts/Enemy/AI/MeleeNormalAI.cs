using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        _enemy.Anim.SetBool("Trace", true);
        _enemy.Agent.SetDestination(_blackboard.target.transform.position);
    }

    public void OnUpdate()
    {
        if (TargetInRay())
        {
            _enemy.SetState(_enemy.actionState);
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
