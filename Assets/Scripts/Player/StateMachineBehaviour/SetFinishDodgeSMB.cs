using System.Collections;
using System.Collections.Generic;
using hvvan;
using UnityEngine;

public class SetFinishDodgeSMB : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.Instance.Player.AbilitySystem.DeleteTag("Invincibility");
    }
}
