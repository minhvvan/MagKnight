using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateDead : BaseState<Enemy>
{
    private EnemyBlackboard _blackboard;
    private Effector _effector;

    private bool _isDeadEffectEnded = false;
    
    public EnemyStateDead(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
        _effector = controller.Effector;
    }

    public override void Enter()
    {
        _effector.Dissolve(3f, () =>
        {
            _isDeadEffectEnded = true;
        });
        
        _controller.Anim.ResetTrigger("Stagger");
        _controller.Anim.SetTrigger("Dead");
        _controller.Rb.Sleep();
        _controller.MainCollider.enabled = false;
    }

    public override void UpdateState()
    {
        if (_controller.IsCurrentAnimFinished("Dead") && _isDeadEffectEnded)
        {
            _controller.gameObject.Destroy();
        }
    }

    public override void Exit()
    {
        
    }
}
