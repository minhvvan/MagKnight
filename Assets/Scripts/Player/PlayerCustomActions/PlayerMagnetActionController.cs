using System;
using System.Collections;
using DG.Tweening;
using Moon;
using UnityEngine;

public class PlayerMagnetActionController : MonoBehaviour
{
    PlayerController _playerController;
    Animator _animator;

    public PlayerController PlayerController => _playerController;

    [SerializeField] ElectricLine _electricLine;
    public ElectricLine ElectricLine => _electricLine;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        _electricLine.gameObject.SetActive(false);
        InitializeMagnetActionStates();
    }

    public Vector3 GetCenterPosition(Transform target)
    {
        if (target.TryGetComponent<Collider>(out var targetCollider))
        {
            var centerPos = targetCollider.bounds.center;
            return centerPos; 
        }
        
        return target.position;
    }


    public void StartMagnetDash(MagneticObject caster, bool isAttack, bool isJump)
    {
        _animator.Play(PlayerAnimatorConst.hashMagnetSkillDash, 0, 0.2f);

        var playerPos = transform.position;
        var playerCenterPos = GetCenterPosition(transform);
        
        var casterMagneticPoint = caster.magneticTargetPoint;
        var casterPos = casterMagneticPoint.position;

        _playerController.inMagnetSkill = true;
        _playerController.inMagnetActionJump = true;
        _playerController.InputHandler.ReleaseControl();

        float distance = Vector3.Distance(playerPos, casterPos);
        float speed = 30f;
        float dashDuration =  distance / speed;
        float hitTiming = Mathf.Clamp(dashDuration - 0.15f, 0, dashDuration);
        bool isCloseTarget = distance < 3f;

        Sequence sequence = DOTween.Sequence();
        
        // Step 1: 살짝 멈춤
        sequence.Append(transform.DOMove(playerPos, 0.05f)
            .SetEase(Ease.OutCubic)
            .OnStart(() => {
                VFXManager.Instance.TriggerVFX(VFXType.MAGNET_ACTION_EXPLOSION, playerCenterPos); 

                _electricLine.ShowEffect(playerCenterPos, casterPos);

                Time.timeScale = 0.2f;
                if(!isCloseTarget){
                    StartCoroutine(_playerController.cameraSettings.AdjustFOV(50f, 100, 0.2f));
                    VolumeController.MotionBlurPlay(0.8f, 0.1f);
                } 
            })
            .OnComplete(() => {
                Time.timeScale = 1f;
                _electricLine.HideEffect();
            })); 

        //회전
        sequence.Join(transform.DOLookAt(casterPos, 0.05f, AxisConstraint.Y)
                .SetEase(Ease.OutCubic));

        //딜레이
        sequence.AppendInterval(0.1f).AppendCallback(() => {
            if(!isCloseTarget){
                StartCoroutine(_playerController.cameraSettings.AdjustFOV(100f, 50f, 0.2f));
            }
        });

        // Step 2: 대쉬 시작
        sequence.AppendCallback(()=>{                
            //너무 가까운 경우 돌진 효과 없음
            if(!isCloseTarget) 
            {
                Quaternion rotation = Quaternion.LookRotation(casterPos - playerPos);
                
                VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_RED, transform.position, rotation);
                VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_BLUE, transform.position, rotation);
            }

            StartCoroutine(MagnetDashCoroutine(casterMagneticPoint, dashDuration,() => {
                    VolumeController.MotionBlurPlay(0, 0.1f);
                    Time.timeScale = 1f;
                }));
            
            if(isAttack)
            {
                StartCoroutine(MagnetDashAttack(hitTiming));
            }

            if(isJump)
            {
                StartCoroutine(MagnetDashJump(dashDuration, 5f, 0.2f));
            }
            
        });
    }

#region Util functions
    Vector3 GetTargetFrontPosition(Transform target, Vector3 startPos){
        var targetCollider = target.GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
        var targetWidth = targetCollider.bounds.size.x;
        var toTargetVector = startPos - targetCenterPos;
        var toTargetVectorRemoveY = new Vector3(toTargetVector.x, 0f, toTargetVector.z);
        Vector3 targetFrontPos = targetCenterPos + toTargetVectorRemoveY.normalized * targetWidth * 1.5f - (targetCollider.bounds.size.y * 0.55f * Vector3.up);

        return targetFrontPos;
    }
#endregion

    IEnumerator MagnetDashCoroutine(Transform destinationTransform, float duration, Action onComplete, EasingFunction.Ease easeType = EasingFunction.Ease.Linear)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            var EaseFunction = EasingFunction.GetEasingFunction(easeType);
            
            Vector3 nextPos = Vector3.Lerp(startPos, destinationTransform.position, EaseFunction(0, 1, t));
            Vector3 delta = nextPos - transform.position;
            _playerController.characterController.Move(delta);

            yield return null;
        }

        // 마지막 위치로 이동
        Vector3 finalPos = destinationTransform.position;
        Vector3 finalDelta = finalPos - transform.position;
        if (finalDelta.magnitude > 0f)
        {
            _playerController.characterController.Move(finalDelta);
        }

        // 종료 처리
        onComplete?.Invoke();
    }


    IEnumerator MagnetDashAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        VFXManager.Instance.TriggerVFX(VFXType.MAGNET_ACTION_EXPLOSION, GetCenterPosition(transform), Quaternion.identity);
        _playerController.StartNormalAttack();
        
        yield return new WaitForSeconds(0.8f);
        
        _playerController.inMagnetSkill = false;
        _playerController.inMagnetActionJump = false;
        _playerController.InputHandler.GainControl();
    }

    IEnumerator MagnetDashJump(float delay, float jumpHeight, float duration)
    {
        yield return new WaitForSeconds(delay);
        VFXManager.Instance.TriggerVFX(VFXType.MAGNET_ACTION_EXPLOSION, GetCenterPosition(transform), Quaternion.identity);
        VFXManager.Instance.TriggerVFX(VFXType.JUMP_DUST, transform.position, Quaternion.identity);
        _playerController.inMagnetSkill = false;
        _playerController.InputHandler.GainControl();
        _animator.Play(PlayerAnimatorConst.hashAirborne, 0, 0.2f);
        float elapsed = 0f;
        float startY = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentHeight = Mathf.Lerp(0f, jumpHeight, t);
            float deltaY = currentHeight - startY;

            Vector3 move = Vector3.up * deltaY;
            _playerController.characterController.Move(move);

            startY = currentHeight;
            elapsed += Time.deltaTime;
            yield return null;
        }

        float finalDelta = jumpHeight - startY;
        if (finalDelta > 0f)
        {
            _playerController.characterController.Move(Vector3.up * finalDelta);
        }

        //_playerController.inMagnetActionJump = false;

        _currentVelocity = jumpHeight * Vector3.up;


        SetMagnetActionState(magnetActionInertiaState, new MagnetActionInertiaStateData(_currentVelocity));
    }


#region Magnet Swing

    public void ResetPlayerState()
    {
        _playerController.inMagnetActionJump = false;
        _playerController.inMagnetSkill = false;
        _playerController.InputHandler.GainControl();
        _electricLine.HideEffect();
        VolumeController.MotionBlurPlay(0.0f, 0.0f);
    }

    public void StartMagnetSwing(MagneticObject caster)
    {
        SetMagnetActionState(magnetActionApproachToTransformState, new MagnetActionApproachToTransformStateData(caster.magneticTargetPoint));
    }

    public void EndSwingWithInertia()
    {        
        if(_currentMagnetActionState == magnetActionSwingState || _currentMagnetActionState == magnetActionApproachToTransformState)
        {
            SetMagnetActionState(magnetActionInertiaState, new MagnetActionInertiaStateData(_currentVelocity));    
        }
    }    
#endregion

#region Magnet action StateMachine Controller
    Vector3 _currentVelocity = Vector3.zero;
    public Vector3 CurrentVelocity
    {
        get => _currentVelocity;
        set => _currentVelocity = value;
    }
    
    public ICustomActionState magnetActionIdleState;
    public ICustomActionState magnetActionInertiaState;
    public ICustomActionState magnetActionSwingState;
    public ICustomActionState magnetActionApproachToTransformState;

    ICustomActionState _currentMagnetActionState;

    public void SetMagnetActionState(ICustomActionState newState, IStateData stateData = null)
    {
        _currentMagnetActionState?.Exit();

        _currentMagnetActionState = newState;
        _currentMagnetActionState.Enter(stateData);
    }

    public void UpdateMagnetActionState()
    {
        _currentMagnetActionState?.UpdateState();
    }

    public void InitializeMagnetActionStates()
    {
        magnetActionIdleState = new MagnetActionIdleState(this);
        magnetActionInertiaState = new MagnetActionInertiaState(this);
        magnetActionSwingState = new MagnetActionSwingState(this);
        magnetActionApproachToTransformState = new MagnetActionApproachToTransformState(this);

        SetMagnetActionState(magnetActionIdleState);
    }

    void LateUpdate()
    {
        UpdateMagnetActionState();
    }
#endregion

#region MagnetPlate
public void StartMagnetPlate(MagneticObject caster, MagneticObject target, bool isGetPlate)
    {
        if (_playerController.inMagnetSkill) return;
        
        var playerPos = transform.position;
        var playerCenterPos = GetCenterPosition(transform);
        
        var casterMagneticPoint = caster.magneticPoint;
        var casterPos = casterMagneticPoint.position;

        MagnetPlate magnetPlate = null;
        
        if (isGetPlate)
        {
            magnetPlate = caster.magnetPlate;
            casterMagneticPoint = magnetPlate.magneticPoint;
            casterPos = magnetPlate.transform.position;
        }
        else
        {
            if (caster.TryGetComponent(out magnetPlate))
            {
                target.magnetPlate = magnetPlate;
            }
        }
        
        var targetMagneticPoint = target.magneticPoint;
        var targetPos = targetMagneticPoint.position;
        
        //_playerController.inMagnetSkill = true;

        float distance = 0f;
        switch (isGetPlate)
        {
            case false:
                distance = Vector3.Distance(playerPos, casterPos);
                break;
            case true:
                distance = Vector3.Distance(casterPos, targetPos);
                break;
        }
        
        float speed = 5f;
        float duration =  distance / speed;
        
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(()=>
        {
            switch (isGetPlate)
            {
                case false:
                    StartCoroutine(MagnetPlatePullCoroutine(casterMagneticPoint, duration, 
                        () => StartCoroutine(MagnetPlateHoldCoroutine(casterMagneticPoint, magnetPlate)
                        )));
                    break;
                case true:
                    StartCoroutine(MagnetPlateThrowCoroutine(
                        magnetPlate, targetMagneticPoint, duration));
                    break;
            }
        });
    }

    Vector3 GetPlayerFrontPosition(Collider playerCollider, float distance = 1f)
    {
        var offset = new Vector3(0, playerCollider.bounds.size.y / 2f, distance);
        var playerFrontPos = transform.TransformPoint(offset);
        return playerFrontPos;
    }
    
    //철판 당겨오기
    IEnumerator MagnetPlatePullCoroutine(Transform plateTransform, float duration, Action onComplete = null, EasingFunction.Ease easeType = EasingFunction.Ease.Linear)
    {
        float elapsed = 0f;
        var playerCollider = transform.GetComponent<Collider>();
        var playerCenterPos = Vector3.zero;
        var handle = _playerController.WeaponHandler.GetHandTransform();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            var EaseFunction = EasingFunction.GetEasingFunction(easeType);

            playerCenterPos = playerCollider.bounds.center;
            var playerFrontPos = GetPlayerFrontPosition(playerCollider);
            
            Vector3 nextPos = Vector3.Lerp(plateTransform.position, playerFrontPos, EaseFunction(0, 1, t));
            plateTransform.LookAt(playerCenterPos);
            plateTransform.position = nextPos;
            if(handle != null) _electricLine.ShowEffect(handle.position, plateTransform.position);
            yield return null;
        }

        // 마지막 위치로 이동
        Vector3 finalPos = GetPlayerFrontPosition(playerCollider);
        Vector3 finalDelta = finalPos - plateTransform.position;
        if (finalDelta.magnitude > 0f)
        {
            plateTransform.LookAt(playerCenterPos);
            plateTransform.position = finalPos;
        }
        
        // 종료 처리
        onComplete?.Invoke();
    }

    //철판 전방에 홀딩
    IEnumerator MagnetPlateHoldCoroutine(Transform plateTransform, MagnetPlate plate)
    {
        var playerCollider = transform.GetComponent<Collider>();
        var handle = _playerController.WeaponHandler.GetHandTransform();
        plate.rb.isKinematic = true;
        while (true)
        {
            //평타 공격,  plate의 hold가 해제될때
            if (_playerController.CanMagneticPlateHoldCancel())
            {
                _electricLine.HideEffect();
                plate.isHold = false;
                plate.rb.isKinematic = false;
                yield break;
            }
            if (!plate.isHold)
            {
                plate.rb.isKinematic = false;
                yield break;
            }

            var playerFrontPos = GetPlayerFrontPosition(playerCollider);
            plateTransform.LookAt(playerCollider.bounds.center);
            plateTransform.position = playerFrontPos;
            
            if(handle != null) _electricLine.ShowEffect(handle.position, plateTransform.position);
            yield return null;
        }
    }
    
    //철판 던지기
    IEnumerator MagnetPlateThrowCoroutine(MagnetPlate plate, Transform destinationTransform, float duration, Action onComplete = null, EasingFunction.Ease easeType = EasingFunction.Ease.Linear)
    {
        float elapsed = 0f;
        var handle = _playerController.WeaponHandler.GetHandTransform();
        var plateTransform = plate.magneticPoint;
        plate.OnHitDetect(true);

        var rotationSpeed = 10f;
        
        plate.transform.Rotate(new Vector3(45f,0,0));
        var rotateSpeed = new Vector3(0,0,600f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
                
            var EaseFunction = EasingFunction.GetEasingFunction(easeType);
            Vector3 nextPos = Vector3.Lerp(plateTransform.position, destinationTransform.position, EaseFunction(0, 1, t));
            plateTransform.position = nextPos;
            plate.transform.Rotate(rotateSpeed * Time.deltaTime);
            //이동 도중 충돌하면 종료
            if (CheckBoxCollision(plate)) break;
            
            if(handle != null) _electricLine.ShowEffect(handle.position, plateTransform.position);
            yield return null;
        }
        _electricLine.HideEffect();
        
        yield return new WaitForSeconds(0.2f);
        
        plate.OnHitDetect(false);
        
        
        // 종료 처리
        onComplete?.Invoke();
    }

    public bool CheckBoxCollision(MagneticObject magneticObject, bool isPlayer = false)
    {
        var playerLayer = LayerMask.NameToLayer("Player");
        var magneticLayer = LayerMask.NameToLayer("Magnetic");
        var enemyLayer = LayerMask.NameToLayer("Enemy");
        var groundLayer = LayerMask.NameToLayer("Ground");
        var environmentLayer = LayerMask.NameToLayer("Environment");

        var obj = magneticObject.gameObject;
        var col = magneticObject.GetComponent<BoxCollider>();
        var center = col.bounds.center;
        var size = col.bounds.size;
        var halfExtents = size / 2f;

        var hitColliders = new Collider[20];
        var hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, hitColliders, Quaternion.identity,
            isPlayer ? playerLayer : 
            (1 << magneticLayer) | (1 << enemyLayer) |
            (1 << groundLayer) | (1 << environmentLayer));

        //감지한 대상 기반으로 보정, 충돌 찾기 수행.
        for (int i = 0; i < hitCount; i++)
        {
            var hitCol = hitColliders[i];

            //본인 제외
            if (hitCol == col) continue;

            //ComputePenetration
            if (Physics.ComputePenetration(col, obj.transform.position, obj.transform.rotation,
                    hitCol, hitCol.transform.position, hitCol.transform.rotation,
                    out Vector3 direction, out float distance))
            {
                //겹침(침투) 보정 //겹친만큼 반대방향으로 이동시킴.
                obj.transform.position += direction * distance;
            }

            return true;
        }

        return false;
    }

    #endregion
}