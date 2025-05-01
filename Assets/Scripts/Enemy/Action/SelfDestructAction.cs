using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructAction : IEnemyAction
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private float _startupDuration;

    private bool _actionRunned;
    
    public SelfDestructAction(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = enemy.blackboard;
    }
    
    public void OnEnter()
    {
        _enemy.Anim.SetTrigger("Action");
        _startupDuration = 0f;
        _blackboard.projectilePrefab.GetComponent<AttackEffect>().OnHit += _enemy.GiveDamageEffect;
    }

    public void OnUpdate()
    {
        _startupDuration += Time.deltaTime;
        if (_startupDuration > _blackboard.startupTime && !_actionRunned)
        {
            _enemy.Anim.SetTrigger("ActionRun");
            _actionRunned = true;
        }
    }

    public void OnExit()
    {
    }
}
