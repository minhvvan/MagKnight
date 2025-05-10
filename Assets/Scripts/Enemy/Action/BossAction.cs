using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BossAction : IEnemyAction
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private float _startupDuration;
    private PatternController _patternController;
    private bool _actionRunned = false;

    public BossAction(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = enemy.blackboard;
        _patternController = enemy.GetComponent<PatternController>();
    }
    
    public void OnEnter()
    {
        _blackboard.actionRecoveryCancellation = new CancellationTokenSource();
        _enemy.SetAnimTrigger("Action");
        _startupDuration = 0f;
    }

    public void OnUpdate()
    {
        if (_actionRunned) return;
        
        _startupDuration += Time.deltaTime;
        if (_startupDuration > _blackboard.startupTime)
        {
            _patternController.ExecutePattern(_enemy.Anim);
            _enemy.SetAnimTrigger("ActionRun");
            _actionRunned = true;
        }
    }

    public void OnExit()
    {
        _actionRunned = false;
        _blackboard.actionRecoveryCancellation.Cancel();
    }
}
