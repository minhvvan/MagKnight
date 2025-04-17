using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        _blackboard.staggerRecoveryCancellation = new CancellationTokenSource();
        _controller.Anim.SetTrigger("Stagger");
    }

    public override void UpdateState()
    {
    }

    public override void Exit()
    {
        _blackboard.staggerRecoveryCancellation.Cancel();
    }
}
