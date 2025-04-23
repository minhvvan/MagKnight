using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RangedNormalAction : IEnemyAction
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private float _startupDuration;
    
    private bool _shot = false;
    
    public RangedNormalAction(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = enemy.blackboard;
    }
    
    public void OnEnter()
    {
        _blackboard.actionRecoveryCancellation = new CancellationTokenSource();
        _enemy.Anim.SetTrigger("Action");
        _startupDuration = 0f;
    }

    public void OnUpdate()
    {
        if (!_shot && _enemy.Anim.GetCurrentAnimatorStateInfo(0).IsName("ActionRunning"))
        {
            _shot = true;
            ProjectileFactory.Create(_blackboard.projectilePrefab, _blackboard.muzzleTransform.position, Quaternion.identity,
                _blackboard.abilitySystem, targetCollider: _blackboard.target.GetComponent<Collider>());
        }
        
        _startupDuration += Time.deltaTime;
        if (_startupDuration > _blackboard.startupTime)
        {
            _enemy.Anim.SetTrigger("ActionRun");
        }
    }

    public void OnExit()
    {
        _blackboard.actionRecoveryCancellation.Cancel();
        _shot = false;
    }
}