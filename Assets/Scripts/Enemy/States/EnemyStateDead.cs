using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VFolders.Libs;

public class EnemyStateDead : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    
    
    public EnemyStateDead(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _controller.Anim.Play("Dead");
        _controller.Rb.Sleep();
        _controller.MainCollider.enabled = false;
    }

    public override void UpdateState()
    {
        if (_controller.IsCurrentAnimFinished("Dead"))
        {
            _controller.gameObject.Destroy();
        }
        
    }

    public override void Exit()
    {
        
    }
}
