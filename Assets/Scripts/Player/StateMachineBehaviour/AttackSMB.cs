using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moon
{
    public class AttackSMB : StateMachineBehaviour
    {
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            PlayerController controller = animator.GetComponent<PlayerController>();

            if (controller != null)
            {
                controller.MeleeAttackEnd();
            }
        }
    } 
}
