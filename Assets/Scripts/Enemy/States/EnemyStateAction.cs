using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyStateAction : BaseState<Enemy>
{
    // 공격, 자폭, 치료 등 각 enemy가 가지고 있는 행동양식 실행
    private EnemyBlackboard _blackboard;
    
    
    public EnemyStateAction(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
    }

    public override void Enter()
    {
        Debug.Log("Enter Action State");
        _blackboard.actionDelayCancellation = new CancellationTokenSource();
        _controller.Anim.SetTrigger("Attack");
    }

    public override void UpdateState()
    {

    }

    public override void Exit()
    {
    }
}
