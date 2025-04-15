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
        _controller.Anim.SetBool("Spawn", true);
    }

    public override void UpdateState()
    {
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
        _controller.Anim.SetBool("Spawn", false);
    }
}
