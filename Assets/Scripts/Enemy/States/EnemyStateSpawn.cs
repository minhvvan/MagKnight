using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyStateSpawn : BaseState<Enemy>
{
    // 맨 처음 실행
    // spawn animation 재생
    // target 확정
    
    private EnemyBlackboard _blackboard;
    
    public EnemyStateSpawn(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _blackboard.target = GameObject.FindWithTag("Player");
    }

    public override void UpdateState()
    {
        if (_blackboard.currentHealth <= 0)
        {
            _controller.SetState(_controller.deadState);
            return;
        }
        
        // 경직성공
        if (_blackboard.currentStaggerResistance <= 0)
        {
            _controller.SetState(_controller.hitState);
            _blackboard.currentStaggerResistance = _blackboard.staggerResistance;
        }
        
        
        if (_controller.IsCurrentAnimFinished("Spawn"))
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
