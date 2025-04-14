using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateAI : BaseState<Enemy>
{
    // attack 후의 state
    // 사거리 내에 target이 있으면 action으로 넘어간다
    private EnemyBlackboard _blackboard;
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer = 0f;
    
    public EnemyStateAI(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _controller.EnemyAnimator.SetBool("Trace", true);
        _controller.Agent.SetDestination(_blackboard.target.transform.position);
    }

    public override void UpdateState()
    {
        if (_blackboard.currentHealth <= 0)
        {
            _controller.SetState(_controller.deadState);
            return;
        }
        
        if (_blackboard.currentStaggerResistance <= 0)
        {
            _controller.EnemyAnimator.Play("Hit");
            _blackboard.currentStaggerResistance = _blackboard.staggerResistance;
            return;
        }
        
        
        if(_controller.TargetInRange())
        {
            _controller.EnemyAnimator.SetFloat("Speed", 0);
        }
        else
        {
            _controller.EnemyAnimator.SetFloat("Speed", 1);
        }
        if (_controller.TargetInRay())
        {
            _controller.SetState(_controller.actionState);
        }
        else
        {
            _destinationTimer += Time.deltaTime;
            if (_destinationTimer >= destinationUpdateInterval)
            {
                _controller.Agent.SetDestination(_blackboard.target.transform.position);
                _destinationTimer = 0f;
            }
        }
    }

    public override void Exit()
    {
        _controller.EnemyAnimator.SetBool("Trace", false);
    }
}
