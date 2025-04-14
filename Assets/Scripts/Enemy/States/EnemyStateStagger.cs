using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateStagger : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    
    public EnemyStateStagger(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _controller.EnemyAnimator.Play("Stagger");
    }

    public override void UpdateState()
    {
        if (_controller.IsCurrentAnimFinished("Stagger"))
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
