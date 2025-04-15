using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAnimatorStateStagger : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        DelayAfterStagger(animator);
    }
    
    private async UniTask DelayAfterStagger(Animator animator)
    {
        Enemy enemy = animator.gameObject.GetComponent<Enemy>();
        await UniTask.Delay(TimeSpan.FromSeconds(enemy.blackboard.staggerRecoveryTime));
        enemy.SetState(enemy.aiState);
    }
}