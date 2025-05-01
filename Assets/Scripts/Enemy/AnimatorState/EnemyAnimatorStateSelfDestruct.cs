using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAnimatorStateSelfDestruct : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Enemy>().blackboard.enemyRenderer.enabled = false;
        DelayedDestroy(animator.gameObject).Forget();
    }

    private async UniTask DelayedDestroy(GameObject gameObject)
    {
        await UniTask.WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
