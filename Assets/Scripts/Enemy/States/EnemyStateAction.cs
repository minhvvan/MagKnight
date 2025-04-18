using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyStateAction : BaseState<Enemy>
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private EnemyBlackboard _blackboard;
    private float _startupDuration;

    private bool _shot = false;
    
    public EnemyStateAction(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _blackboard.actionRecoveryCancellation = new CancellationTokenSource();
        _controller.Anim.SetTrigger("Action");
        _startupDuration = 0f;
    }

    public override void UpdateState()
    {
        if (!_shot && _controller.Anim.GetCurrentAnimatorStateInfo(0).IsName("ActionRunning") && _blackboard.attackType == EnemyAttckType.Ranged)
        {
            _shot = true;
            GameObject projectile = GameObject.Instantiate(_blackboard.projectilePrefab, _blackboard.muzzleTransform.position, Quaternion.identity);
            projectile.GetComponent<Projectile>().Initialize(_blackboard);
        }
        
        _startupDuration += Time.deltaTime;
        if (_startupDuration > _blackboard.startupTime)
        {
            _controller.Anim.SetTrigger("ActionRun");
        }
    }

    public override void Exit()
    {
        _blackboard.actionRecoveryCancellation.Cancel();
        _shot = false;
    }
}
