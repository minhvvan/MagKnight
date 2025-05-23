using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeleeNormalAction : IEnemyAction
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private float _startupDuration;
    private bool _actionRunned;
    
    public MeleeNormalAction(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = enemy.blackboard;
    }
    
    public void OnEnter()
    {
        _blackboard.actionRecoveryCancellation = new CancellationTokenSource();
        _enemy.SetAnimTrigger("Action");
        // _enemy.Anim.SetTrigger("Action");
        _startupDuration = 0f;
    }

    public void OnUpdate()
    {
        _startupDuration += Time.deltaTime;
        if (_startupDuration > _blackboard.startupTime && !_actionRunned)
        {
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
