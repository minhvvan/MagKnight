using System.Collections;
using System.Collections.Generic;
using hvvan;
using Moon;
using UnityEngine;

public class SetFinishSkillSMB : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.Instance.Player.AbilitySystem.DeleteTag("SuperArmor");
    }
}
