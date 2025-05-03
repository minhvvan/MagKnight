using UnityEngine;

public class MagnetActionSwingState : BaseMagnetActionState
{
    float _ropeLength;
    Transform _anchorTransform;


    //values
    private float gravity = 9.81f;          // 중력 가속도
    private float maxTangentialSpeed = 25f; // 최대 접선 속도
    private float maxSwingAngle = 240;     //최대 스윙 각도를 올리면 끊어질때 위로 올라감 (260이 재미있었음)
    private const float MIN_DELTA_TIME = 0.001f;  // 최소 deltaTime

    //local variables
    float _swingElapsed;
    float _currentAngle;
    Vector3 _swingDirection;
    float _angularVelocity;    
    float _maxAngularVelocity;


    public MagnetActionSwingState(PlayerMagnetActionController controller) : base(controller)
    {
    }

    public override void Enter(IStateData stateData = null)
    {   
        if (stateData is MagnetActionSwingStateData swingStateData)
        {
            _anchorTransform = swingStateData.anchorTransform;
            _ropeLength = swingStateData.ropeLength;
        }

        _swingElapsed = 0f;

        Vector3 swingStartPos = controller.transform.position;
        Vector3 initialVelocity = controller.CurrentVelocity;
        Vector3 toAnchor = _anchorTransform.position - swingStartPos;

        _swingDirection = Vector3.Cross(toAnchor.normalized, Vector3.up).normalized;
        float initialAngularVelocity = initialVelocity.magnitude / _ropeLength;
        _angularVelocity = initialAngularVelocity;

        Vector3 ropeDir = (controller.transform.position - _anchorTransform.position).normalized;
        Vector3 swingAxis = Vector3.Cross(ropeDir, Vector3.up).normalized;
        Vector3 reference = Vector3.ProjectOnPlane(Vector3.up, swingAxis).normalized;
        Vector3 projectedRopeDir = Vector3.ProjectOnPlane(ropeDir, swingAxis).normalized;
        _currentAngle = Vector3.SignedAngle(projectedRopeDir, reference, swingAxis);

        _maxAngularVelocity = maxTangentialSpeed / _ropeLength;

        Debug.Log("Swing State Enter");
    }

    public override void Exit()
    {
        Debug.Log("Swing State Exit");    
    }

    public override void UpdateState()
    {
        bool reachedPeak = false;

        float gravityAccel = gravity * Mathf.Sin(_currentAngle * Mathf.Deg2Rad) / _ropeLength;
            
        if (gravityAccel > 0)
        {
            _angularVelocity += gravityAccel * Time.deltaTime * 1.5f;
        }
        else
        {
            _angularVelocity = Mathf.Max(_angularVelocity, 0);
        }
        
        _angularVelocity = Mathf.Clamp(_angularVelocity, -_maxAngularVelocity, _maxAngularVelocity);
        
        _currentAngle += _angularVelocity * Mathf.Rad2Deg * Time.deltaTime;

        Quaternion rotation = Quaternion.AngleAxis(_currentAngle, _swingDirection);
        Vector3 swingOffset = rotation * Vector3.up * _ropeLength;
        Vector3 targetPos = _anchorTransform.position + swingOffset;

        Vector3 moveDir = targetPos - controller.transform.position;
        float deltaTime = Mathf.Max(Time.deltaTime, MIN_DELTA_TIME);
        controller.CurrentVelocity = moveDir / deltaTime;

        if (controller.CurrentVelocity.magnitude > maxTangentialSpeed)
            controller.CurrentVelocity = controller.CurrentVelocity.normalized * maxTangentialSpeed;

        controller.PlayerController.characterController.Move(moveDir);
        controller.ElectricLine.ShowEffect(controller.GetCenterPosition(controller.transform), controller.GetCenterPosition(_anchorTransform));

        if (Mathf.Abs(_currentAngle) > maxSwingAngle)
        {
            reachedPeak = true;
        }

        _swingElapsed += Time.deltaTime;

        if (reachedPeak)
        {
            MagnetActionInertiaStateData stateData = new MagnetActionInertiaStateData(controller.CurrentVelocity);
            controller.SetMagnetActionState(controller.magnetActionInertiaState, stateData);
            return;
        }

        if(_swingElapsed >= 3)
        {
            MagnetActionInertiaStateData stateData = new MagnetActionInertiaStateData(controller.CurrentVelocity);
            controller.SetMagnetActionState(controller.magnetActionInertiaState, stateData);
            return;
        }
    }
}
