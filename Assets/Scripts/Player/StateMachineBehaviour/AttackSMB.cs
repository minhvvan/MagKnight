using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moon
{
    public class AttackSMB : StateMachineBehaviour
    {
        private bool _hasBlendOutStarted = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            PlayerController controller = animator.GetComponent<PlayerController>();
            if(controller.WeaponHandler.CurrentWeaponType == WeaponType.Bow)
            {
                controller.SetForceRotationToAim();
            }
            else
            {
                controller.SetForceRotation();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_hasBlendOutStarted) return;

            if (animator.IsInTransition(layerIndex))
            {
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(layerIndex);
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(layerIndex);
                
                if (stateInfo.fullPathHash == current.fullPathHash &&
                    stateInfo.fullPathHash != next.fullPathHash
                )
                {
                    _hasBlendOutStarted = true;

                    AttackEnd(animator);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //블렌딩 아웃이 시작되지 않았다면, 애니메이션 상태가 종료될 때 공격 종료 메서드를 호출
            if(!_hasBlendOutStarted)
            {
                AttackEnd(animator);
            }

            _hasBlendOutStarted = false;
        }

        void AttackEnd(Animator animator)
        {
            PlayerController controller = animator.GetComponent<PlayerController>();
            //controller?.MeleeAttackEnd();
            controller?.WeaponHandler.AttackEnd(0);
            controller?.WeaponHandler.AttackEnd(1);
        }
    } 
}
