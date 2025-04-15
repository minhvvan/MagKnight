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
        Debug.Log("Enter Stagger State");
        _blackboard.actionDelayCancellation.Cancel();
        _controller.Anim.SetTrigger("Stagger");
    }

    public override void UpdateState()
    {
    }

    public override void Exit()
    {
        Debug.Log("Exit Stagger State");
    }
}
