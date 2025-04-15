using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAnimatorStateAction : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        DelayAfterAttack(animator);
    }
    private async UniTask DelayAfterAttack(Animator animator)
    {
        Enemy enemy = animator.gameObject.GetComponent<Enemy>();
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(enemy.blackboard.recoveryTime),
                cancellationToken: enemy.blackboard.actionDelayCancellation.Token);

            // 딜레이가 정상적으로 끝났을 때만 실행됨
            enemy.SetState(enemy.aiState);
        }
        catch (OperationCanceledException)
        {
            // 토큰이 취소됐을 때 -> 아무 것도 안 함
        }
    }
}
