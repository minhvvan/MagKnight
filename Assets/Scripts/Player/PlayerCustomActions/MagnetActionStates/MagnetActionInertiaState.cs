using UnityEngine;

public class MagnetActionInertiaState : BaseMagnetActionState
{
    Vector3 _finalVelocity;

    //values
    float _inertiaTime = 0.5f;

    //local variables
    float _inertiaElapsed = 0f;

    public MagnetActionInertiaState(PlayerMagnetActionController controller) : base(controller)
    {
    }

    public override void Enter(IStateData stateData = null)
    {
        _inertiaElapsed = 0f;

        if (stateData is MagnetActionInertiaStateData inertiaStateData)
        {
            _finalVelocity = inertiaStateData.finalVelocity;
            _finalVelocity.y = _finalVelocity.y * 0.3f;
        }

        controller.PlayerController.inMagnetActionJump = true;

        controller.ElectricLine.HideEffect();
        VolumeController.MotionBlurPlay(0.0f, 0.1f);
    }

    public override void Exit()
    {
        controller.PlayerController.inMagnetActionJump = false;
    }

    public override void UpdateState()
    {
        //접근한 동안에 무조건 Player Controller에서 적용한 중력 무시
        controller.PlayerController.inMagnetActionJump = true;

        float t = _inertiaElapsed / _inertiaTime;
        float easeOut = 1f - (t * t * t); 
        Vector3 inertiaVelocity = _finalVelocity * easeOut;

        controller.PlayerController.characterController.Move(inertiaVelocity * Time.deltaTime);

        _inertiaElapsed += Time.deltaTime;

        if(_inertiaElapsed >= _inertiaTime)
        {
            controller.SetMagnetActionState(controller.magnetActionIdleState);
            return;
        }

        if(controller.PlayerController.IsGrounded)
        {
            controller.SetMagnetActionState(controller.magnetActionIdleState);
            return;
        }
    }
}
