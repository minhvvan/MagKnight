using UnityEngine;

public class MagnetActionApproachToTransformState : BaseMagnetActionState
{
    float _elapsed;
    Transform _toTransform; //Anchor Transform


    private float _swingStartDistance = 4f;  // 스윙 시작 거리
    private float _initialAccelSpeed = 15;  // 초기 이동 속도

    public MagnetActionApproachToTransformState(PlayerMagnetActionController controller) : base(controller)
    {
    }

    public override void Enter(IStateData stateData = null)
    {
        if (stateData is MagnetActionApproachToTransformStateData approachStateData)
        {
            _toTransform = approachStateData.toTransform;
        }

        _elapsed = 0f;

        controller.ResetPlayerState();
        VFXManager.Instance.TriggerVFX(VFXType.MAGNET_ACTION_EXPLOSION, controller.GetCenterPosition(controller.transform), Quaternion.identity);

        controller.PlayerController.inMagnetActionJump = true;
        controller.PlayerController.inMagnetSkill = true;
        //controller.PlayerController.InputHandler.ReleaseControl();
        VolumeController.MotionBlurPlay(0.8f, 0.1f);
        // controller.StartCoroutine(controller.PlayerController.cameraSettings.AdjustFOV(50, 30, 0.5f));

        controller.PlayerController.LookAtForce(_toTransform, true);
    }

    public override void Exit()
    {

    }

    public override void UpdateState()
    {  
        Vector3 moveDir = (_toTransform.position - controller.transform.position).normalized;
        float targetSpeed = _initialAccelSpeed;
        
        if (controller.CurrentVelocity.magnitude > targetSpeed)
        {
            targetSpeed = controller.CurrentVelocity.magnitude;
        }
        
        controller.CurrentVelocity = moveDir * targetSpeed;
        controller.PlayerController.characterController.Move(controller.CurrentVelocity * 3 * Time.deltaTime);
        
        controller.ElectricLine.ShowEffect(controller.GetCenterPosition(controller.transform), controller.GetCenterPosition(_toTransform));
        
        _elapsed += Time.deltaTime;

        float distanceToAnchor = Vector3.Distance(controller.transform.position, _toTransform.position);
        
        if (distanceToAnchor <= _swingStartDistance * 1.2)
        {
            MagnetActionSwingStateData stateData = new MagnetActionSwingStateData(_toTransform, distanceToAnchor);
            controller.SetMagnetActionState(controller.magnetActionSwingState, stateData);
            return;
        }

        if (_elapsed >= 1.5f)
        {
            controller.SetMagnetActionState(controller.magnetActionIdleState);
            return;
        }
    }
}
