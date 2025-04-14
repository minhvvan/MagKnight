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
        _enemy.EnemyAnimator.SetBool("Trace", true);
        _enemy.Agent.SetDestination(_blackboard.target.transform.position);
    }

    public void OnUpdate()
    {
        if(_enemy.TargetInRange())
        {
            _enemy.EnemyAnimator.SetFloat("Speed", 0);
        }
        else
        {
            _enemy.EnemyAnimator.SetFloat("Speed", 1);
        }
        if (_enemy.TargetInRay())
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
        _enemy.EnemyAnimator.SetBool("Trace", false);
    }
}
