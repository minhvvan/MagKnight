using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateHit : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    
    public EnemyStateHit(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _controller.EnemyAnimator.SetBool("Hit", true);
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
        }

        if (_controller.IsCurrentAnimFinished("Hit"))
        {
            if (_controller.TargetInRay())
                _controller.SetState(_controller.actionState);
            else
                _controller.SetState(_controller.aiState);
        }
    }

    public override void Exit()
    {
        
    }
}
