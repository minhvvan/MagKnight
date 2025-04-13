using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateAI : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    
    
    public EnemyStateAI(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        
    }

    public override void UpdateState()
    {
        
    }

    public override void Exit()
    {
        
    }
}
