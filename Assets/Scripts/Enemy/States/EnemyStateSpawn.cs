using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateSpawn : BaseState<Enemy>
{
    // spawn animation 재생
    // target 확정
    
    private EnemyBlackboard _blackboard;
    
    public EnemyStateSpawn(Enemy controller) : base(controller)
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
