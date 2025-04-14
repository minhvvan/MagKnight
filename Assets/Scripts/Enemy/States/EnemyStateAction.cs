using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateAction : BaseState<Enemy>
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private EnemyBlackboard _blackboard;
    
    
    public EnemyStateAction(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _controller.EnemyAnimator.SetBool("Attack", true);
    }

    public override void UpdateState()
    {
        if (_blackboard.currentHealth <= 0)
        {
            _controller.SetState(_controller.deadState);
            return;
        }
        
        if (_blackboard.currentStaggerResistance <= 0)
        {
            _controller.EnemyAnimator.Play("Hit");
            _blackboard.currentStaggerResistance = _blackboard.staggerResistance;
            return;
        }

        if (_controller.IsCurrentAnimFinished("Attack"))
        {
            if (!_controller.TargetInRay())
                _controller.SetState(_controller.aiState);
            else
            {
                _controller.EnemyAnimator.Play("Attack");
            }
        }
    }

    public override void Exit()
    {
        _controller.EnemyAnimator.SetBool("Attack", false);
    }
}
