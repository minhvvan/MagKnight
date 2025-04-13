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
        
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }
}
