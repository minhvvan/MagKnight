using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAnimatorStateStagger : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        DelayAfterStagger(animator).Forget();
    }
    
    private async UniTask DelayAfterStagger(Animator animator)
    {
        Enemy enemy = animator.gameObject.GetComponent<Enemy>();
        
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(enemy.blackboard.staggerRecoveryTime),
                cancellationToken: enemy.blackboard.staggerRecoveryCancellation.Token);

            // 딜레이가 정상적으로 끝났을 때만 실행됨
            enemy.SetState(enemy.aiState);
        }
        catch (OperationCanceledException)
        {
            // 토큰이 취소됐을 때 -> 아무 것도 안 함
        }
    }
}