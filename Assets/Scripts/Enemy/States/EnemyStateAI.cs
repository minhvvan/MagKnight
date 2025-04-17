using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateAI : BaseState<Enemy>
{
    // 모든 action 뒤에는 AI state를 한번 거친다
    // 사거리 내에 target이 있으면 action으로 넘어간다
    private EnemyBlackboard _blackboard;
    
    public EnemyStateAI(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        _blackboard.ai.OnEnter();
    }

    public override void UpdateState()
    {
        _blackboard.ai.OnUpdate();
    }

    public override void Exit()
    {
        _blackboard.ai.OnExit();
    }
}
