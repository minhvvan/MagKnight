using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateDead : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    
    
    public EnemyStateDead(Enemy controller) : base(controller)
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
