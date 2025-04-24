using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using hvvan;
using Jun;
using UnityEngine;

namespace Moon
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IInteractor
    {
        private CharacterController _characterController;
        private Animator _animator;
        private InputHandler _inputHandler;
        private MagneticController _magneticController;
        private InteractionController _interactionController;
        private WeaponHandler _weaponHandler;
        private AbilitySystem _abilitySystem;
        Collider _collider;

        [SerializeField] private GameObject hudPrefab;

        [SerializeField] public float maxForwardSpeed = 8f;        
        [SerializeField] public float gravity = 20f;               
        [SerializeField] public float jumpSpeed = 10f;             
        [SerializeField] public float minTurnSpeed = 400f;         
        [SerializeField] public float maxTurnSpeed = 1200f;        
        [SerializeField] public float idleTimeout = 5f;            
        [SerializeField] public bool canAttack;

        public CameraSettings cameraSettings;
        public bool isDead;

        LockOnSystem _lockOnSystem;
        bool _lockOnLastFrame = false;

        protected AnimatorStateInfo _currentStateInfo;    // Information about the base layer of the animator cached.
        protected AnimatorStateInfo _nextStateInfo;
        protected bool _isAnimatorTransitioning;
        protected AnimatorStateInfo _previousCurrentStateInfo;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo _previousNextStateInfo;
        protected bool _previousIsAnimatorTransitioning;
        protected bool _isGrounded = true;            // Whether or not Ellen is currently standing on the ground.
        protected bool _previouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.
        protected bool _readyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.
        protected bool _isKnockDown = false;
        protected bool _isKnockDownFront = false;
        protected float _desiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.
        protected float _forwardSpeed;                // How fast Ellen is currently going along the ground.
        protected float _verticalSpeed;               // How fast Ellen is currently moving up or down.
        protected Collider[] _overlapResult = new Collider[8];    // Used to cache colliders
        
        protected Quaternion _targetRotation;         // What rotation Ellen is aiming to have based on input.
        protected float _angleDiff;                   // Angle in degrees between Ellen's current rotation and her target rotation.
        protected bool _inAttack;                     // Whether Ellen is currently in the middle of a melee attack.
        protected bool _inCombo;                      // Whether Ellen is currently in the middle of her melee combo.        
        protected float _idleTimer;                   // Used to count up to Ellen considering a random idle.

        // These constants are used to ensure Ellen moves and behaves properly.
        // It is advised you don't change them without fully understanding what they do in code.
        const float k_AirborneTurnSpeedProportion = 5.4f;
        const float k_GroundedRayDistance = 1f;
        const float k_JumpAbortSpeed = 10f;
        const float k_InverseOneEighty = 1f / 180f;
        const float k_StickingGravityProportion = 0.3f;
        const float k_GroundAcceleration = 20f;
        const float k_GroundDeceleration = 25f;
        const float k_MinEnemyDotCoeff = 0.2f;

        // Parameters

        readonly int _HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        readonly int _HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        readonly int _HashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
        readonly int _HashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
        readonly int _HashGrounded = Animator.StringToHash("Grounded");
        readonly int _HashInputDetected = Animator.StringToHash("InputDetected");
        readonly int _HashMeleeAttack = Animator.StringToHash("MeleeAttack");
        readonly int _HashHurt = Animator.StringToHash("Hurt");
        readonly int _HashDeath = Animator.StringToHash("Death");
        readonly int _HashRespawn = Animator.StringToHash("Respawn");
        readonly int _HashHurtFromX = Animator.StringToHash("HurtFromX");
        readonly int _HashHurtFromY = Animator.StringToHash("HurtFromY");
        readonly int _HashStateTime = Animator.StringToHash("StateTime");
        readonly int _HashFootFall = Animator.StringToHash("FootFall");
        readonly int _HashAttackType = Animator.StringToHash("AttackType");
        readonly int _HashLockOn = Animator.StringToHash("LockOn");
        readonly int _HashMoveX   = Animator.StringToHash("MoveX");
        readonly int _HashMoveY   = Animator.StringToHash("MoveY");
        readonly int _HashSpeed   = Animator.StringToHash("Speed");
        readonly int _HashBigHurt = Animator.StringToHash("BigHurt");

        // States
        readonly int _HashLocomotion = Animator.StringToHash("Locomotion");
        readonly int _HashLockOnWalk = Animator.StringToHash("LockOnWalk");
        readonly int _HashLockOnJog = Animator.StringToHash("LockOnJog");
        readonly int _HashAirborne = Animator.StringToHash("Airborne");
        readonly int _HashLanding = Animator.StringToHash("Landing");
        readonly int _HashEllenCombo1 = Animator.StringToHash("EllenCombo1");
        readonly int _HashEllenCombo2 = Animator.StringToHash("EllenCombo2");
        readonly int _HashEllenCombo3 = Animator.StringToHash("EllenCombo3");
        readonly int _HashEllenCombo4 = Animator.StringToHash("EllenCombo4");
        readonly int _HashEllenCombo5 = Animator.StringToHash("EllenCombo5");
        readonly int _HashEllenCombo6 = Animator.StringToHash("EllenCombo6");
        readonly int _HashEllenCombo1_Charge = Animator.StringToHash("EllenCombo1 Charge");
        readonly int _HashEllenCombo2_Charge = Animator.StringToHash("EllenCombo2 Charge");
        readonly int _HashEllenCombo3_Charge = Animator.StringToHash("EllenCombo3 Charge");
        readonly int _HashEllenCombo4_Charge = Animator.StringToHash("EllenCombo4 Charge");
        readonly int _HashEllenCombo5_Charge = Animator.StringToHash("EllenCombo5 Charge");
        readonly int _HashEllenCombo6_Charge = Animator.StringToHash("EllenCombo6 Charge");

        // Tags
        readonly int _HashBlockInput = Animator.StringToHash("BlockInput");

        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(_inputHandler.MoveInput.sqrMagnitude, 0f); }
        }

        void UpdateMoveParameters()
        {
            Vector2 moveInput = _inputHandler.MoveInput;
            _animator.SetFloat(_HashMoveX, moveInput.x, 0.2f, Time.deltaTime);
            _animator.SetFloat(_HashMoveY, moveInput.y, 0.2f, Time.deltaTime);
            
            float speed = moveInput.magnitude;
            _animator.SetFloat(_HashSpeed, speed);
        }
        
        public void SetCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }

        // Called automatically by Unity when the script is first added to a gameobject or is reset from the context menu.
        void Reset()
        {
            cameraSettings = FindObjectOfType<CameraSettings>();

            if (cameraSettings != null)
            {
                if (cameraSettings.follow == null)
                    cameraSettings.follow = transform;

                if (cameraSettings.lookAt == null)
                    cameraSettings.follow = transform.Find("HeadTarget");
            }
        }

        async void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            _characterController = GetComponent<CharacterController>();
            _magneticController = GetComponent<MagneticController>();
            _lockOnSystem = GetComponent<LockOnSystem>();
            _abilitySystem = GetComponent<AbilitySystem>();
            _weaponHandler = GetComponent<WeaponHandler>();
            _interactionController = GetComponentInChildren<InteractionController>();
            
            var stat = await GameManager.Instance.GetPlayerStat();
            _abilitySystem.InitializeFromPlayerStat(stat);
            if (_abilitySystem.TryGetAttributeSet<PlayerAttributeSet>(out var attributeSet))
            {
                attributeSet.OnDead += Death;
                attributeSet.OnDamaged += Damaged;
            }
            
            //HUD 생성 및 바인딩
            var hud = Instantiate(hudPrefab);
            if (hud.TryGetComponent<InGameUIController>(out var inGameUIController))
            {
                inGameUIController.BindAttributeChanges(_abilitySystem);
            }

            _inputHandler.magneticInput = MagneticPress;
            _inputHandler.magneticOutput = MagneticRelease;
            _inputHandler.SwitchMangeticInput = SwitchMagneticInput;
        }

        // Called automatically by Unity once every Physics step.
        void FixedUpdate()
        {
            CacheAnimatorState();

            UpdateInputBlocking();
            UpdateCameraHandler();

            EquipMeleeWeapon(IsInAttackComboState());

            _animator.SetFloat(_HashStateTime, Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            _animator.ResetTrigger(_HashMeleeAttack);

            if (_inputHandler.Attack1 && canAttack)
            {
                _animator.SetTrigger(_HashMeleeAttack);
                _animator.SetInteger(_HashAttackType, 0);
            }


            if (_inputHandler.Attack2 && canAttack)
            {
                _animator.SetTrigger(_HashMeleeAttack);
                _animator.SetInteger(_HashAttackType, 1);
            }

            if (_inputHandler.InteractInput)
            {
                Interact();
                _inputHandler.InteractInput = false;
            }
            
            UpdateMoveParameters();
            
            bool lockOnNow = _inputHandler.LockOnInput;  // true/false
            // 눌린 순간(!이전 && 지금)
            if (lockOnNow && !_lockOnLastFrame)
            {
                _lockOnSystem.ToggleLockOn();
                if (_lockOnSystem.currentTarget != null)
                {
                    _animator.SetTrigger(_HashLockOn);
                }
                else
                {
                    _animator.ResetTrigger(_HashLockOn);
                }
            }
            // 상태 저장
            _lockOnLastFrame = lockOnNow;
            

            SetGrounded();

            CalculateForwardMovement();
            CalculateVerticalMovement();

            SetTargetRotation();

            if (IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();

            //PlayAudio();

            
            TimeoutToIdle();

            
        }

        // Called at the start of FixedUpdate to record the current state of the base layer of the animator.
        void CacheAnimatorState()
        {
            _previousCurrentStateInfo = _currentStateInfo;
            _previousNextStateInfo = _nextStateInfo;
            _previousIsAnimatorTransitioning = _isAnimatorTransitioning;

            _currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _nextStateInfo = _animator.GetNextAnimatorStateInfo(0);
            _isAnimatorTransitioning = _animator.IsInTransition(0);
        }

        // Called after the animator state has been cached to determine whether this script should block user input.
        void UpdateInputBlocking()
        {
            bool inputBlocked = _currentStateInfo.tagHash == _HashBlockInput && !_isAnimatorTransitioning;
            inputBlocked |= _nextStateInfo.tagHash == _HashBlockInput;
            _inputHandler.playerControllerInputBlocked = inputBlocked;
        }

        void UpdateCameraHandler()
        {
            if(_inputHandler.IsContollerInputBlocked())
            {
                cameraSettings.DisableCameraMove();
                if(isDead)
                {
                    AnimateCameraYAxis(cameraSettings.Current, 1f, 1f, this.GetCancellationTokenOnDestroy()).Forget();
                }
            }
            else
            {
                cameraSettings.EnableCameraMove();
            }
        }

        async UniTask AnimateCameraYAxis(CinemachineFreeLook camera, float targetValue, float duration, CancellationToken token)
        {
            float startValue = camera.m_YAxis.Value;
            float time = 0f;

            while (time < duration)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                camera.m_YAxis.Value = Mathf.Lerp(startValue, targetValue, t);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }

            camera.m_YAxis.Value = targetValue;
        }
        
        bool IsInAttackComboState()
        {
            bool equipped = _nextStateInfo.shortNameHash == _HashEllenCombo1 || _currentStateInfo.shortNameHash == _HashEllenCombo1;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo2 || _currentStateInfo.shortNameHash == _HashEllenCombo2;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo3 || _currentStateInfo.shortNameHash == _HashEllenCombo3;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo4 || _currentStateInfo.shortNameHash == _HashEllenCombo4;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo5 || _currentStateInfo.shortNameHash == _HashEllenCombo5;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo6 || _currentStateInfo.shortNameHash == _HashEllenCombo6;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo1_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo1_Charge;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo2_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo2_Charge;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo3_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo3_Charge;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo4_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo4_Charge;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo5_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo5_Charge;
            equipped |= _nextStateInfo.shortNameHash == _HashEllenCombo6_Charge || _currentStateInfo.shortNameHash == _HashEllenCombo6_Charge;
            

            return equipped;
        }

        void EquipMeleeWeapon(bool equip)
        {
            //meleeWeapon.gameObject.SetActive(equip);
            _inAttack = false;
            _inCombo = equip;

            if (!equip)
                _animator.ResetTrigger(_HashMeleeAttack);
        }

        // Called each physics step.
        void CalculateForwardMovement()
        {
            // Cache the move input and cap it's magnitude at 1.
            Vector2 moveInput = _inputHandler.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            // Calculate the speed intended by input.
            _desiredForwardSpeed = moveInput.magnitude *  (_inputHandler.RunInput ?  maxForwardSpeed : maxForwardSpeed / 2);

            // Determine change to speed based on whether there is currently any move input.
            float acceleration = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            // Adjust the forward speed towards the desired speed.
            _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredForwardSpeed, acceleration * Time.deltaTime);

            // Set the animator parameter to control what animation is being played.
            _animator.SetFloat(_HashForwardSpeed, _forwardSpeed);
        }

        // Called each physics step.
        void CalculateVerticalMovement()
        {
            if (!_inputHandler.JumpInput && _isGrounded)
                _readyToJump = true;

            if (_isGrounded)
            {
                //땅에 붙도록
                _verticalSpeed = -gravity * k_StickingGravityProportion;
                
                if (_inputHandler.JumpInput && _readyToJump && !_inCombo)
                {                    
                    _verticalSpeed = jumpSpeed;
                    _isGrounded = false;
                    _readyToJump = false;
                }
            }
            else
            {
                // If Ellen is airborne, the jump button is not held and Ellen is currently moving upwards...
                if (!_inputHandler.JumpInput && _verticalSpeed > 0.0f)
                {
                    _verticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                if (Mathf.Approximately(_verticalSpeed, 0f))
                {
                    _verticalSpeed = 0f;
                }                
          
                _verticalSpeed -= gravity * Time.deltaTime;
            }
        }

        
        void SetTargetRotation()
        {
            Vector2 moveInput = _inputHandler.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (localMovementDirection == Vector3.zero) return;

            Quaternion targetRotation;

            // 락온 중이면 새로운 계산, 그렇지 않으면 기존 FreeLook 계산
            if (_lockOnSystem.currentTarget != null)
            {
                // --- 락온 모드 계산 ---
                // 실제 락온 카메라를 기준으로 forward 추출
                Transform camT = cameraSettings.lockOnCamera.transform;
                Vector3 forward = camT.forward;
                forward.y = 0f; forward.Normalize();

                if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1f))
                    targetRotation = Quaternion.LookRotation(-forward);
                else
                {
                    var offset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                    targetRotation = Quaternion.LookRotation(offset * forward);
                }
            }
            else
            {
                // --- FreeLook 모드(원래 코드) ---
                Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
                forward.y = 0f; forward.Normalize();

                if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1f))
                    targetRotation = Quaternion.LookRotation(-forward);
                else
                {
                    var offset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                    targetRotation = Quaternion.LookRotation(offset * forward);
                }
            }

            // 공통: 최종 forward, 애니메이션용 angleDiff 계산
            Vector3 resultingForward = targetRotation * Vector3.forward;
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle  = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;
            _angleDiff      = Mathf.DeltaAngle(angleCurrent, targetAngle);
            _targetRotation = targetRotation;
        }


        // Called each physics step to help determine whether Ellen can turn under player input.
        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLocomotion || _nextStateInfo.shortNameHash == _HashLocomotion;
            bool updateOrientationForAirborne = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashAirborne || _nextStateInfo.shortNameHash == _HashAirborne;
            bool updateOrientationForLanding = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLanding || _nextStateInfo.shortNameHash == _HashLanding;
            // ★ 락온 워크/조그 상태 추가
            bool updateForLockOnWalk = 
                (!_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLockOnWalk) 
                || (_nextStateInfo.shortNameHash == _HashLockOnWalk);
            bool updateForLockOnJog  = 
                (!_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLockOnJog) 
                || (_nextStateInfo.shortNameHash == _HashLockOnJog);

            return updateForLockOnWalk || updateForLockOnJog || updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || _inCombo && !_inAttack;
        }

        void SetGrounded()
        {   
            _isGrounded = _characterController.isGrounded;
            
            if (!_isGrounded && !_previouslyGrounded)
                _animator.SetFloat(_HashAirborneVerticalSpeed, _verticalSpeed);

            
            _animator.SetBool(_HashGrounded, _isGrounded || _previouslyGrounded);

            _previouslyGrounded = _isGrounded;
        }

        
        void UpdateOrientation()
        {
            // 1) 락온 중이면, 대상 바라보기만 하고 리턴
            if (_lockOnSystem.currentTarget != null)
            {
                Vector3 dir = _lockOnSystem.currentTarget.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
                return;
            }

            // 2) 평소 locomotion / airborne / landing 회전 처리
            _animator.SetFloat(_HashAngleDeltaRad, _angleDiff * Mathf.Deg2Rad);

            if (_currentStateInfo.shortNameHash == _HashLocomotion)
            {
                Vector3 localInput = new Vector3(_inputHandler.MoveInput.x, 0f, _inputHandler.MoveInput.y);
                float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, _forwardSpeed / _desiredForwardSpeed);
                float actualTurnSpeed = _isGrounded
                    ? groundedTurnSpeed
                    : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty 
                                                                   * k_AirborneTurnSpeedProportion * groundedTurnSpeed;

                _targetRotation = Quaternion.RotateTowards(
                    transform.rotation, _targetRotation, actualTurnSpeed * Time.deltaTime
                );
                transform.rotation = _targetRotation;
            }
        }


#if false  //오디오 관련 비활성화
        // Called each physics step to check if audio should be played and if so instruct the relevant random audio player to do so.
        void PlayAudio()
        {
            float footfallCurve = _Animator.GetFloat(_HashFootFall);

            if (footfallCurve > 0.01f && !footstepPlayer.playing && footstepPlayer.canPlay)
            {
                footstepPlayer.playing = true;
                footstepPlayer.canPlay = false;
                footstepPlayer.PlayRandomClip(_CurrentWalkingSurface, _ForwardSpeed < 4 ? 0 : 1);
            }
            else if (footstepPlayer.playing)
            {
                footstepPlayer.playing = false;
            }
            else if (footfallCurve < 0.01f && !footstepPlayer.canPlay)
            {
                footstepPlayer.canPlay = true;
            }

            if (_IsGrounded && !m_PreviouslyGrounded)
            {
                landingPlayer.PlayRandomClip(_CurrentWalkingSurface, bankId: _ForwardSpeed < 4 ? 0 : 1);
                emoteLandingPlayer.PlayRandomClip();
            }

            if (!m_IsGrounded && _PreviouslyGrounded && _VerticalSpeed > 0f)
            {
                emoteJumpPlayer.PlayRandomClip();
            }

            if (_CurrentStateInfo.shortNameHash == _HashHurt && _PreviousCurrentStateInfo.shortNameHash != _HashHurt)
            {
                hurtAudioPlayer.PlayRandomClip();
            }

            if (_CurrentStateInfo.shortNameHash == _HashEllenDeath && _PreviousCurrentStateInfo.shortNameHash != _HashEllenDeath)
            {
                emoteDeathPlayer.PlayRandomClip();
            }

            if (_CurrentStateInfo.shortNameHash == _HashEllenCombo1 && _PreviousCurrentStateInfo.shortNameHash != _HashEllenCombo1 ||
                _CurrentStateInfo.shortNameHash == _HashEllenCombo2 && _PreviousCurrentStateInfo.shortNameHash != _HashEllenCombo2 ||
                _CurrentStateInfo.shortNameHash == _HashEllenCombo3 && _PreviousCurrentStateInfo.shortNameHash != _HashEllenCombo3 ||
                _CurrentStateInfo.shortNameHash == _HashEllenCombo4 && _PreviousCurrentStateInfo.shortNameHash != _HashEllenCombo4)
            {
                emoteAttackPlayer.PlayRandomClip();
            }
        }
#endif
        // Called each physics step to count up to the point where Ellen considers a random idle.
        void TimeoutToIdle()
        {
            bool inputDetected = IsMoveInput || _inputHandler.Attack1 || _inputHandler.Attack2 || _inputHandler.JumpInput;
            if (_isGrounded && !inputDetected)
            {
                _idleTimer += Time.deltaTime;

                if (_idleTimer >= idleTimeout)
                {
                    _idleTimer = 0f;
                    _animator.SetTrigger(_HashTimeoutToIdle);
                }
            }
            else
            {
                _idleTimer = 0f;
                _animator.ResetTrigger(_HashTimeoutToIdle);
            }

            _animator.SetBool(_HashInputDetected, inputDetected);
        }
        
        void OnAnimatorMove()
        {
            // 1) 콤보 중엔 항상 루트 모션만
            if (_inCombo)
            {
                // 애니메이터에 박혀있는 이동(deltaPosition)만 적용
                Vector3 move = _animator.deltaPosition;
                // 필요하면 중력/점프 속도 추가
                move += Vector3.up * _verticalSpeed * Time.deltaTime;
                _characterController.Move(move);
                return;
            }

            // 2) 락온 중이고 콤보가 아니면, 입력 기반 이동
            if (_lockOnSystem.currentTarget != null)
            {
                Vector3 dirToTarget = _lockOnSystem.currentTarget.position - transform.position;
                dirToTarget.y = 0f;
                transform.rotation = Quaternion.LookRotation(dirToTarget);

                Vector2 inp = _inputHandler.MoveInput;
                Vector3 localInput = new Vector3(inp.x, 0f, inp.y);
                if (localInput.sqrMagnitude > 1f) localInput.Normalize();

                Vector3 move = (transform.right * localInput.x
                                + transform.forward * localInput.z)
                               * _forwardSpeed * Time.deltaTime;
                move += Vector3.up * _verticalSpeed * Time.deltaTime;
                _characterController.Move(move);
                return;
            }

            
            Vector3 movement;

            if (isDead) return;

            if (_isGrounded)
            {
                // ★ Locomotion 뿐 아니라 LockOnWalk/Jog 도 같이
                bool isStdMove = _currentStateInfo.shortNameHash == _HashLocomotion;

                if (isStdMove)
                {
                    movement = _forwardSpeed * transform.forward * Time.deltaTime;
                }
                else
                {
                    // 기존 Root Motion 처리
                    RaycastHit hit;
                    Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                    if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                        movement = Vector3.ProjectOnPlane(_animator.deltaPosition, hit.normal);
                    else
                        movement = _animator.deltaPosition;
                }
            }
            else
            {
                movement = _forwardSpeed * transform.forward * Time.deltaTime;
            }
            
            _characterController.transform.rotation *= _animator.deltaRotation;
            movement += _verticalSpeed * Vector3.up * Time.deltaTime;
            _characterController.Move(movement);

            Collider[] colliders = Physics.OverlapCapsule(transform.position, transform.position + Vector3.up * 1.8f, 0.4f, LayerMask.GetMask("Enemy"));

            foreach(Collider hitCol in colliders)
            {

                if (Physics.ComputePenetration(_collider, transform.position, transform.rotation,
                    hitCol, hitCol.transform.position, hitCol.transform.rotation,
                    out Vector3 direction, out float distance))
                {
                    //transform.position += direction * distance;

                    transform.position = Vector3.Lerp(transform.position, transform.position + (direction * distance), 1f);

                }
            }
        }
        

        void Interact()
        {
            if (_interactionController != null)
            {
                _interactionController.Interact();
            }
        }

        void SwitchMagneticInput()
        {
            if (_magneticController != null)
            {
                _magneticController.SwitchMagneticType();
            }
        }

        void MagneticPress()
        {
            if (_magneticController != null)
            {
                _magneticController.OnPressEnter();
            }
        }
        void MagneticRelease(bool inputValue)
        {
            if (_magneticController == null) return;
            if(inputValue) _magneticController.OnLongRelease().Forget(); 
            else _magneticController.OnShortRelease().Forget();
        }

        public void MeleeAttackStart(int throwing = 0)
        {
            _weaponHandler.AttackStart();
        }

        public void MeleeAttackEnd()
        {
            _weaponHandler.AttackEnd();
        }
        
        #region Weapon
        public void SetCurrentWeapon(WeaponType weaponType)
        {
            _weaponHandler.SetCurrentWeapon(weaponType);
        }
        #endregion
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        
        public void RespawnFinished()
        {
            isDead = false;
        }

        public void StandFinished()
        {
            _isKnockDown = false;
        }

        public void Death()
        {
            if(isDead) return;
            
            _abilitySystem.TriggerEvent(TriggerEventType.OnDeath, _abilitySystem);
            if(_abilitySystem.GetValue(AttributeType.HP) <= 0)
            {
                Debug.Log("Dead Player");
                isDead = true;
                _animator.SetTrigger(_HashDeath);
                GameManager.Instance.ChangeGameState(GameState.GameOver);
            }
        }

        public void Damaged(Transform sourceTransform)
        {
            if (isDead || _isKnockDown) return;

            if(sourceTransform != null)
            {
                // 공격이 들어온 방향
                var dir = sourceTransform.position - transform.position;
                // 내 캐릭터에 방향에 맞게 계산
                var hurtFrom = transform.InverseTransformDirection(dir.normalized);
                _animator.SetFloat(_HashHurtFromX, hurtFrom.x);
                _animator.SetFloat(_HashHurtFromY, hurtFrom.z);
                
                var impulse = _abilitySystem.GetValue(AttributeType.Impulse);
                // 충격량이 임계값보다 작음 : 그냥 경직
                if (impulse > 0)
                {
                    _animator.SetTrigger(_HashHurt);
                }
                // 큰 경직
                else
                {
                    _animator.SetTrigger(_HashBigHurt); 
                    _isKnockDown = true;
                }
            }
        }

        void OnAnimatorIK(int layerIndex)
        {
            if(_animator.GetBool(_HashLockOn) && _lockOnSystem.currentTarget != null)
            {
                Transform target = _lockOnSystem.currentTarget;
                _animator.SetLookAtPosition(target.position + Vector3.up * 1.5f);
                _animator.SetLookAtWeight(0.6f);
            }
        }
    }
}