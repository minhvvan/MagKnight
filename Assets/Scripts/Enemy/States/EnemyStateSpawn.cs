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
    private Effector _effector;
    
    private bool _isSpawnEffectEnded = false;
    
    public EnemyStateSpawn(Enemy controller) : base(controller)
    {
        _blackboard = controller.blackboard;
        _effector = controller.Effector;
    }

    public override void Enter()
    {
        _controller.Anim.SetBool("Spawn", true);
        
        _effector.Phase(2f, ()=>
        {
            _isSpawnEffectEnded = true;
        });
    }

    public override void UpdateState()
    {
        if (_controller.IsCurrentAnimFinished("Spawn") && _isSpawnEffectEnded)
        {
            _controller.SetState(_controller.aiState);
            
            if (_blackboard is BossBlackboard bossBlackboard)
            {
                bossBlackboard.BindHPBar();
            }
        }
    }

    public override void Exit()
    {
        _blackboard.target = GameObject.FindWithTag("Player");
        _controller.Anim.SetBool("Spawn", false);
    }
}
