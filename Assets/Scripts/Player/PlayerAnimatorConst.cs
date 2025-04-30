using UnityEngine;

public static class PlayerAnimatorConst
{
    // Parameters
    public readonly static int hashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
    public readonly static int hashForwardSpeed = Animator.StringToHash("ForwardSpeed");
    public readonly static int hashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
    public readonly static int hashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
    public readonly static int hashGrounded = Animator.StringToHash("Grounded");
    public readonly static int hashInputDetected = Animator.StringToHash("InputDetected");
    public readonly static int hashMeleeAttack = Animator.StringToHash("MeleeAttack");
    public readonly static int hashHurt = Animator.StringToHash("Hurt");
    public readonly static int hashDeath = Animator.StringToHash("Death");
    public readonly static int hashRespawn = Animator.StringToHash("Respawn");
    public readonly static int hashHurtFromX = Animator.StringToHash("HurtFromX");
    public readonly static int hashHurtFromY = Animator.StringToHash("HurtFromY");
    public readonly static int hashStateTime = Animator.StringToHash("StateTime");
    public readonly static int hashFootFall = Animator.StringToHash("FootFall");
    public readonly static int hashAttackType = Animator.StringToHash("AttackType");
    public readonly static int hashLockOn = Animator.StringToHash("LockOn");
    public readonly static int hashMoveX   = Animator.StringToHash("MoveX");
    public readonly static int hashMoveY   = Animator.StringToHash("MoveY");
    public readonly static int hashSpeed   = Animator.StringToHash("Speed");
    public readonly static int hashBigHurt = Animator.StringToHash("BigHurt");
    public readonly static int hashDodge = Animator.StringToHash("Dodge");
    public readonly static int hashDodgeX = Animator.StringToHash("DodgeX");
    public readonly static int hashDodgeY = Animator.StringToHash("DodgeY");
    public readonly static int hashMoveSpeed = Animator.StringToHash("MoveSpeed");
    public readonly static int hashAttackSpeed = Animator.StringToHash("AttackSpeed");
    public readonly static int hashImpulse = Animator.StringToHash("Impulse");
        
    // States
    public readonly static int hashLocomotion = Animator.StringToHash("Locomotion");
    public readonly static int hashLockOnWalk = Animator.StringToHash("LockOnWalk");
    public readonly static int hashLockOnJog = Animator.StringToHash("LockOnJog");
    public readonly static int hashAirborne = Animator.StringToHash("Airborne");
    public readonly static int hashLanding = Animator.StringToHash("Landing");
    public readonly static int hashEllenCombo1 = Animator.StringToHash("EllenCombo1");
    public readonly static int hashEllenCombo2 = Animator.StringToHash("EllenCombo2");
    public readonly static int hashEllenCombo3 = Animator.StringToHash("EllenCombo3");
    public readonly static int hashEllenCombo4 = Animator.StringToHash("EllenCombo4");
    public readonly static int hashEllenCombo5 = Animator.StringToHash("EllenCombo5");
    public readonly static int hashEllenCombo6 = Animator.StringToHash("EllenCombo6");
    public readonly static int hashEllenCombo1_Charge = Animator.StringToHash("EllenCombo1 Charge");
    public readonly static int hashEllenCombo2_Charge = Animator.StringToHash("EllenCombo2 Charge");
    public readonly static int hashEllenCombo3_Charge = Animator.StringToHash("EllenCombo3 Charge");
    public readonly static int hashEllenCombo4_Charge = Animator.StringToHash("EllenCombo4 Charge");
    public readonly static int hashEllenCombo5_Charge = Animator.StringToHash("EllenCombo5 Charge");
    public readonly static int hashEllenCombo6_Charge = Animator.StringToHash("EllenCombo6 Charge");
    public readonly static int hashMagnetSkillDash = Animator.StringToHash("MagnetSkillDash");
    
    // Tags
    public readonly static int hashBlockInput = Animator.StringToHash("BlockInput");
}