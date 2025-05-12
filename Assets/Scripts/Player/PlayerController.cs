using System;
using System.Threading;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using hvvan;
using JetBrains.Annotations;
using Jun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Moon
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IInteractor
    {
        [SerializeField] private GameObject hudPrefab;
        [SerializeField] public float gravity = 20f;               
        [SerializeField] public float jumpSpeed = 10f;             
        [SerializeField] public float minTurnSpeed = 400f;         
        [SerializeField] public float maxTurnSpeed = 1200f;        
        [SerializeField] public float idleTimeout = 5f;            
        [SerializeField] public bool canAttack;
        [SerializeField] private SerializedDictionary<WeaponType, RuntimeAnimatorController> animatorControllers;
        
        #region Conponent
        [NonSerialized] public CharacterController characterController;
        private Animator _animator;
        private InputHandler _inputHandler;
        private MagneticController _magneticController;
        private InteractionController _interactionController;
        private WeaponHandler _weaponHandler;
        private AbilitySystem _abilitySystem;
        private float _maxForwardSpeed;
        private Collider _collider;
        private LockOnSystem _lockOnSystem;
        private ArtifactInventory _artifactInventory;
        private PlayerMagnetActionController _playerMagnetActionController;
        private Effector _effect;
        #endregion
            
        #region Property
        public WeaponHandler WeaponHandler => _weaponHandler;
        public AbilitySystem AbilitySystem => _abilitySystem;
        public CameraSettings cameraSettings;
        public InputHandler InputHandler => _inputHandler;
        public bool IsInvisible => isInvisible || isDead;
        public bool IsGrounded => _isGrounded;
        
        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(_inputHandler.MoveInput.sqrMagnitude, 0f); }
        }
        #endregion
        
        #region Variable
        bool isDead = false;
        bool isInvisible = true;
        private bool _isInputUpdate = true;
        private bool _isDodging = false;
        [SerializeField] private float parryWindow = 0.2f;
        private bool _parryWindowActive = false;
        Coroutine _parryWindowCoroutine;
        private bool _canSwitchMagnetic = true;
        private Coroutine _switchCooldownCoroutine;
        [NonSerialized] public float switchMagneticCooldown = 5f;
        
        [Header("Parry Slow Motion")]
        [SerializeField] private float parrySlowAmount   = 0.2f;   // 슬로우모션 배율
        [SerializeField] private float parrySlowDuration = 0.5f;   // 슬로우모션 실제 지속시간
        private float _originalFixedDeltaTime;
        private Coroutine _parrySlowCoroutine;


        #endregion

        #region Animation
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
        protected float _desiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.
        protected float _forwardSpeed;                // How fast Ellen is currently going along the ground.
        protected float _verticalSpeed;               // How fast Ellen is currently moving up or down.
        protected Collider[] _overlapResult = new Collider[8];    // Used to cache colliders
        
        protected Quaternion _targetRotation;         // What rotation Ellen is aiming to have based on input.
        protected float _angleDiff;                   // Angle in degrees between Ellen's current rotation and her target rotation.
        protected bool _inAttack;                     // Whether Ellen is currently in the middle of a melee attack.
        protected bool _inCombo;                      // Whether Ellen is currently in the middle of her melee combo.        
        protected float _idleTimer;                   // Used to count up to Ellen considering a random idle.

        #endregion

        [NonSerialized] public bool inMagnetSkill = false;
        [NonSerialized] public bool inMagnetActionJump = false;


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
        const float k_maxForwardSpeed = 8f;
        

        void UpdateMoveParameters()
        {
            Vector2 moveInput = _inputHandler.MoveInput;
            _animator.SetFloat(PlayerAnimatorConst.hashMoveX, moveInput.x, 0.2f, Time.deltaTime);
            _animator.SetFloat(PlayerAnimatorConst.hashMoveY, moveInput.y, 0.2f, Time.deltaTime);
            
            float speed = moveInput.magnitude;
            _animator.SetFloat(PlayerAnimatorConst.hashSpeed, speed);
        }
        
        public void SetCanAttack(bool newCanAttack)
        {
            this.canAttack = newCanAttack;
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

        private async void Awake()
        {
            Reset();

            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            characterController = GetComponent<CharacterController>();
            _magneticController = GetComponent<MagneticController>();
            _lockOnSystem = GetComponent<LockOnSystem>();
            _abilitySystem = GetComponent<AbilitySystem>();
            _weaponHandler = GetComponent<WeaponHandler>();
            _interactionController = GetComponentInChildren<InteractionController>();
            _artifactInventory = GetComponentInChildren<ArtifactInventory>();
            _playerMagnetActionController = GetComponent<PlayerMagnetActionController>();
            _effect = GetComponentInChildren<Effector>();

            if(SceneManager.GetActiveScene().name.StartsWith("Prototype"))
            {
                var stat = await GameManager.Instance.GetPlayerStat();
                InitStat(stat);
                _magneticController.InitializeMagnetic();
            }
                
            _inputHandler.magneticInput = MagneticPress;
            _inputHandler.magneticOutput = MagneticRelease;
            _inputHandler.SwitchMagneticInput = SwitchMagneticInput;
            
            _originalFixedDeltaTime = Time.fixedDeltaTime; // 패링 슬로우모션 용
        }
        
        //명시적 초기화
        public void InitializeByCurrentRunData(CurrentRunData currentRunData)
        {
            InitStat(currentRunData.playerStat);
            
            //무기 지급
            if (currentRunData.currentWeapon == WeaponType.None)
            {
                Debug.Log("CurrentWeapon is None");
                //TODO: 무기가 없으면 베이스캠프로 강제 이동(예외 처리)
            }

            //현재 Magcore 오브젝트를 플레이어 transform에 귀속시켜야 MagCoreData가 유지됨.
            //Magcore만 가져오면 오브젝트가 없어서 Missing발생. -> 추가적으로 기존무기 Drop 로직도 이어지지 않게됨.
            var category = currentRunData.currentItemCategory;
            if (category == ItemCategory.MagCore)//MagCore가 아니면 데이터가 없는 것.
            {
                var rarity = currentRunData.currentItemRarity;
                var itemName = currentRunData.currentItemName;
                var upgradeValue = currentRunData.currentPartsUpgradeValue;
                var magCore = ItemManager.Instance.CreateItem(category, rarity, transform.position, 
                    Quaternion.identity,itemName: itemName, parent: transform);
                var currentSpec = magCore.GetComponent<MagCore>();
                currentSpec.SetMagCoreData(currentRunData.currentMagCoreSO);
                currentSpec.currentUpgradeValue = upgradeValue;
                currentSpec.SetPartsEffect(_abilitySystem);
                magCore.SetActive(false);//외부 영향없게 비활성화
                _weaponHandler.currentMagCore = currentSpec;
            }
            
            SetCurrentWeapon(currentRunData.currentWeapon);
            RecoverArtifact(currentRunData);
        }

        public void InitStat(PlayerStat stat)
        {
            _abilitySystem.InitializeFromPlayerStat(stat);
            
            _maxForwardSpeed = k_maxForwardSpeed * _abilitySystem.GetValue(AttributeType.MoveSpeed);
            if (_abilitySystem.TryGetAttributeSet<PlayerAttributeSet>(out var attributeSet))
            {
                attributeSet.OnDead += Death;
                attributeSet.OnDamaged += Damaged;
                attributeSet.OnImpulse += Impulse;
                attributeSet.OnMoveSpeedChanged += MoveSpeedChanged;
                attributeSet.OnAttackSpeedChanged += AttackSpeedChanged;
                
                // OnHitPassive
                var chargeSkillGaugeHit = new PassiveEffectData
                {
                    effect = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, 3),
                    triggerChance = 1,
                    triggerEvent = TriggerEventType.OnHit
                };
                
                _abilitySystem.RegisterPassiveEffect(chargeSkillGaugeHit);
                
                // OnDamagePassive
                var chargeSkillGauageDamaged = new PassiveEffectData
                {
                    effect = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, 2),
                    triggerChance = 1,
                    triggerEvent = TriggerEventType.OnDamage
                };
                
                _abilitySystem.RegisterPassiveEffect(chargeSkillGauageDamaged);
                
                var chargeSkillGauageMagnetic = new PassiveEffectData
                {
                    effect = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, 2),
                    triggerChance = 1,
                    triggerEvent = TriggerEventType.OnMagnetic
                };
                
                _abilitySystem.RegisterPassiveEffect(chargeSkillGauageMagnetic);
                
                var chargeSkillGauageParry = new PassiveEffectData {
                    effect        = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, 10),
                    triggerChance = 1,
                    triggerEvent  = TriggerEventType.OnParry
                };
                
                _abilitySystem.RegisterPassiveEffect(chargeSkillGauageParry);
                
                var onSkill = new PassiveEffectData
                {
                    effect = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, -500),
                    triggerChance = 1,
                    triggerEvent = TriggerEventType.OnSkill
                };
                
                _abilitySystem.RegisterPassiveEffect(onSkill);
            }
            
            //HUD 생성 및 바인딩
            // var hud = Instantiate(hudPrefab);
            // if (hud.TryGetComponent<InGameUIController>(out var inGameUIController))
            // {
            //     inGameUIController.BindAttributeChanges(_abilitySystem);
            // }
            UIManager.Instance.inGameUIController.UnbindAttributeChanges();
            UIManager.Instance.inGameUIController.BindAttributeChanges(_abilitySystem);
            _magneticController.InitializeMagnetic();
        }
        
        
        // Called automatically by Unity once every Physics step.
        void FixedUpdate()
        {
            CacheAnimatorState();

            UpdateInputBlocking();
            UpdateCameraHandler();

            EquipMeleeWeapon(IsInAttackComboState());

            TriggerDodge();

            TriggerAttack();

            TriggerInteract();

            //임시지정키 G. 아이템 분해
            if (Input.GetKeyDown(KeyCode.G))
            {
                Dismentle();
            }

            //임의로 무기 강화 강제로 올리기.
            // if (Input.GetKeyDown(KeyCode.Alpha0))
            // {
            //     UpgradeParts();
            // }

            TriggerSkill();

            UpdateMoveParameters();

            UpdateLockOn();

            SetGrounded();

            CalculateForwardMovement();
            CalculateVerticalMovement();

            if(!_inCombo)
            {
                SetTargetRotation();
            }

            if (IsOrientationUpdated())
                UpdateOrientation();

            //PlayAudio();

            TimeoutToIdle();
        }

        private void TriggerDodge()
        {
            bool dodgeNow = _inputHandler.DodgeInput && _isGrounded;

            if (dodgeNow)
            {
                _abilitySystem.SetTag("Invincibility");
                PerformDodge();
            }
        }

        private void TriggerAttack()
        {
            if(_weaponHandler.CurrentWeaponType == WeaponType.None)
            {
                return;
            }

            _animator.SetFloat(PlayerAnimatorConst.hashStateTime, Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            _animator.ResetTrigger(PlayerAnimatorConst.hashMeleeAttack);

            if (_inputHandler.Attack1 && canAttack)
            {
                _animator.SetTrigger(PlayerAnimatorConst.hashMeleeAttack);
                _animator.SetInteger(PlayerAnimatorConst.hashAttackType, 0);
            }


            if (_inputHandler.Attack2 && canAttack)
            {
                _animator.SetTrigger(PlayerAnimatorConst.hashMeleeAttack);
                _animator.SetInteger(PlayerAnimatorConst.hashAttackType, 1);
            }
        }

        void TriggerInteract()
        {
            if (_inputHandler.InteractInput)
            {
                Interact();
            }
        }

        void TriggerSkill()
        {
            if(_weaponHandler.CurrentWeaponType == WeaponType.None)
            {
                return;
            }

            if (_inputHandler.SkillInput && _isGrounded)
            {
                if (Mathf.Approximately(_abilitySystem.GetValue(AttributeType.SkillGauge), _abilitySystem.GetValue(AttributeType.MaxSkillGauge)))
                {
                    _abilitySystem.SetTag("SuperArmor");
                    _weaponHandler.ActivateSkill();
                    _abilitySystem.TriggerEvent(TriggerEventType.OnSkill, _abilitySystem);
                }
            }
        }

        void UpdateLockOn()
        {
            if (_inputHandler.LockOnInput)
            {
                _lockOnSystem.ToggleLockOn();
            }

            if (_lockOnSystem.IsLockOn)
            {
                _animator.SetTrigger(PlayerAnimatorConst.hashLockOn);
            }
            else
            {
                _animator.ResetTrigger(PlayerAnimatorConst.hashLockOn);
            }
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
            bool inputBlocked = _currentStateInfo.tagHash == PlayerAnimatorConst.hashBlockInput && !_isAnimatorTransitioning;
            inputBlocked |= _nextStateInfo.tagHash == PlayerAnimatorConst.hashBlockInput;
            _inputHandler.playerControllerInputBlocked = inputBlocked;
        }

        void UpdateCameraHandler()
        {
            if(_inputHandler.IsControllerInputBlocked() && !_isDodging)
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

        async UniTask AnimateCameraYAxis(CinemachineFreeLook cam, float targetValue, float duration, CancellationToken token)
        {
            float startValue = cam.m_YAxis.Value;
            float time = 0f;

            while (time < duration)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                cam.m_YAxis.Value = Mathf.Lerp(startValue, targetValue, t);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }

            cam.m_YAxis.Value = targetValue;
        }
        
        bool IsInAttackComboState()
        {
            bool equipped = _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo1 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo1;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo2 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo2;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo3 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo3;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo4 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo4;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo5 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo5;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo6 || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo6;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo1_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo1_Charge;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo2_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo2_Charge;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo3_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo3_Charge;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo4_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo4_Charge;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo5_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo5_Charge;
            equipped |= _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo6_Charge || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashEllenCombo6_Charge;
            
            return equipped;
        }

        void EquipMeleeWeapon(bool equip)
        {
            //meleeWeapon.gameObject.SetActive(equip);
            _inAttack = false;
            _inCombo = equip;

            if (!equip)
                _animator.ResetTrigger(PlayerAnimatorConst.hashMeleeAttack);
        }

        
        #region Move
        // Called each physics step.
        void CalculateForwardMovement()
        {
            // Cache the move input and cap it's magnitude at 1.
            Vector2 moveInput = _inputHandler.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            // Calculate the speed intended by input.
            _desiredForwardSpeed = moveInput.magnitude *  (_lockOnSystem.IsLockOn ?  _maxForwardSpeed / 2 : _maxForwardSpeed);

            // Determine change to speed based on whether there is currently any move input.
            float acceleration = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            // Adjust the forward speed towards the desired speed.
            if (_isInputUpdate)
            {
                _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredForwardSpeed, acceleration * Time.deltaTime);
            }

            // Set the animator parameter to control what animation is being played.
            _animator.SetFloat(PlayerAnimatorConst.hashForwardSpeed, _forwardSpeed);
        }

        // Called each physics step.
        void CalculateVerticalMovement()
        {
            if (!_inputHandler.JumpInput && _isGrounded)
                _readyToJump = true;

            if (_isGrounded)
            {
                //땅에 붙도록
                //_verticalSpeed = -gravity * k_StickingGravityProportion;
                
                if (_inputHandler.JumpInput && _readyToJump && !_inCombo && !_isDodging)
                {                    
                    _verticalSpeed = jumpSpeed;
                    _isGrounded = false;
                    _readyToJump = false;
                    VFXManager.Instance.TriggerVFX(VFXType.JUMP_DUST, transform.position);
                }
            }
            else if (inMagnetActionJump)
            {
                //스윙 중에 점프키를 눌렀을때 연결을 끊고 관성 적용을 위함
                if(_inputHandler.JumpInput)
                {
                    _playerMagnetActionController.EndSwingWithInertia();
                }

                _verticalSpeed = 0f;
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
            var targetRotation = GetTargetRotationToMovement();

            Vector2 moveInput = _inputHandler.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (localMovementDirection == Vector3.zero) return;

            // 공통: 최종 forward, 애니메이션용 angleDiff 계산
            Vector3 resultingForward = targetRotation * Vector3.forward;
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle  = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;
            _angleDiff      = Mathf.DeltaAngle(angleCurrent, targetAngle);
            _targetRotation = targetRotation;
        }

        public void SetForceRotation()
        {
            if(inMagnetSkill) return;
            
            Vector2 moveInput = _inputHandler.ForceMoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (localMovementDirection == Vector3.zero) return;

            var targetRotation = GetTargetRotationToMovement(true);

            _targetRotation = targetRotation;
            transform.rotation = targetRotation;
        }
        

        Quaternion GetTargetRotationToMovement(bool isForce = false)
        {
            Vector2 moveInput = isForce ? _inputHandler.ForceMoveInput : _inputHandler.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (localMovementDirection == Vector3.zero) return Quaternion.identity;

            Quaternion targetRotation;

            // 락온 중이면 새로운 계산, 그렇지 않으면 기존 FreeLook 계산
            if (_lockOnSystem.IsLockOn)
            {
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

            return targetRotation;
        }

        public void SetForceRotationToAim()
        {
            if (!_lockOnSystem.IsLockOn)
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 rotation = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
                
                transform.rotation = Quaternion.LookRotation(rotation);
                _targetRotation = transform.rotation;
            }
        }


        // Called each physics step to help determine whether Ellen can turn under player input.
        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLocomotion || _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashLocomotion;
            bool updateOrientationForAirborne = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashAirborne || _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashAirborne;
            bool updateOrientationForLanding = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLanding || _nextStateInfo.shortNameHash == PlayerAnimatorConst.hashLanding;
            // ★ 락온 워크/조그 상태 추가
            bool updateForLockOnWalk = 
                (!_isAnimatorTransitioning && _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnWalk) 
                || (_nextStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnWalk);
            bool updateForLockOnJog  = 
                (!_isAnimatorTransitioning && _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnJog) 
                || (_nextStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnJog);

            return updateForLockOnWalk || updateForLockOnJog || updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || (_inCombo && !_inAttack && !inMagnetSkill);
        }

        void SetGrounded()
        {   
            _isGrounded = characterController.isGrounded;

            if (_isGrounded && !_previouslyGrounded && !IsInvisible)
                VFXManager.Instance.TriggerVFX(VFXType.JUMP_DUST, transform.position);

            
            if (!_isGrounded && !_previouslyGrounded)
                _animator.SetFloat(PlayerAnimatorConst.hashAirborneVerticalSpeed, _verticalSpeed);

            
            _animator.SetBool(PlayerAnimatorConst.hashGrounded, _isGrounded || _previouslyGrounded);

            _previouslyGrounded = _isGrounded;
        }

        void UpdateOrientation()
        {
            // 1) 락온 중이면, 대상 바라보기만 하고 리턴
            if (_lockOnSystem.IsLockOn)
            {
                Vector3 dir = _lockOnSystem.currentTarget.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
                return;
            }

            // 2) 평소 locomotion / airborne / landing 회전 처리
            _animator.SetFloat(PlayerAnimatorConst.hashAngleDeltaRad, _angleDiff * Mathf.Deg2Rad);

            //if (_currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLocomotion)
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

        public void MoveForce(Transform targetTransform, Action onComplete = null, Action onFail = null)
        {
            _inputHandler.ReleaseControl();
            _isInputUpdate = false;
            
            //*임시 -> 회전시킨후 직진
            LookAtForce(targetTransform, false, () =>
            {
                StartCoroutine(MoveToTarget(targetTransform, onComplete, onFail));
            });
        }

        private IEnumerator MoveToTarget(Transform targetTransform, Action onComplete, Action onFail = null)
        {
            var currentPosition = transform.position;
            var currentTime = 0f;
            var timeout = 3f;

            _forwardSpeed = 4f;
            while ((currentPosition - targetTransform.position).sqrMagnitude >= .2f && currentTime <= timeout)
            {
                var dir = targetTransform.transform.position - transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir);
                currentTime += Time.deltaTime;
                currentPosition = transform.position;
                yield return null;
            }

            if (currentPosition != targetTransform.position)
            {
                onFail?.Invoke();
            }
            else
            {
                onComplete?.Invoke();
            }

            _forwardSpeed = 0f;
            
            _inputHandler.GainControl();
            _isInputUpdate = true;
        }

        public void LookAtForce(Transform targetTransform, bool runImmediately, Action onComplete = null)
        {
            var dir = targetTransform.transform.position - transform.position;
            dir.y = 0;
            var targetRotation = Quaternion.LookRotation(dir);
            _targetRotation = targetRotation;
            
            if(runImmediately)
            {
                transform.rotation = targetRotation;
                onComplete?.Invoke();
            }
            else
            {
                //*임시
                transform.DORotateQuaternion(targetRotation, 0.5f).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
            }
        }

        #endregion

#if false  //오디오 관련 비활성화
        // Called each physics step to check if audio should be played and if so instruct the relevant random audio player to do so.
        void PlayAudio()
        {
            float footfallCurve = _Animator.GetFloat(PlayerAnimatorConst.HashFootFall);

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

            if (_CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashHurt && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashHurt)
            {
                hurtAudioPlayer.PlayRandomClip();
            }

            if (_CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashEllenDeath && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashEllenDeath)
            {
                emoteDeathPlayer.PlayRandomClip();
            }

            if (_CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashEllenCombo1 && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashEllenCombo1 ||
                _CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashEllenCombo2 && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashEllenCombo2 ||
                _CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashEllenCombo3 && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashEllenCombo3 ||
                _CurrentStateInfo.shortNameHash == PlayerAnimatorConst.HashEllenCombo4 && _PreviousCurrentStateInfo.shortNameHash != PlayerAnimatorConst.HashEllenCombo4)
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
                    _animator.SetTrigger(PlayerAnimatorConst.hashTimeoutToIdle);
                }
            }
            else
            {
                _idleTimer = 0f;
                _animator.ResetTrigger(PlayerAnimatorConst.hashTimeoutToIdle);
            }

            _animator.SetBool(PlayerAnimatorConst.hashInputDetected, inputDetected);
        }
        
        void OnAnimatorMove()
        {
            // Early out: 사망 상태면 이동 중단
            if (isDead)
                return;

            Vector3 movement = Vector3.zero;
            bool useManualMove = CanUseManualMove();
            // 2) 락온 중, 콤보 아님 -> 입력 기반 이동
            if(_lockOnSystem.IsLockOn)
            {
                // 캐릭터 회전: 타겟 바라보기
                // Vector3 toTarget = _lockOnSystem.currentTarget.position - transform.position;
                // toTarget.y = 0f;
                // if (toTarget.sqrMagnitude > 0.001f)
                //     transform.rotation = Quaternion.LookRotation(toTarget);
                
                if (useManualMove)
                {
                    // 이동 벡터: 입력 기준으로
                    Vector2 raw = _inputHandler.MoveInput;
                    Vector3 dir = new Vector3(raw.x, 0f, raw.y);
                    if (dir.sqrMagnitude > 1f) dir.Normalize();

                    movement = (transform.right * dir.x + transform.forward * dir.z) * (_forwardSpeed * Time.deltaTime);
                }
                else
                {
                    movement = GetRootMotionMovement();
                }
            }
            else
            {
                // 3) 일반 로코모션 
                if (useManualMove)
                {
                    movement = transform.forward * (_forwardSpeed * Time.deltaTime);
                }
                else
                {
                    movement = GetRootMotionMovement();
                }
            }


            if(inMagnetSkill)
            {
                //마그넷 컨트롤러에서 제어
            }
            else
            {
                // 5) 회전 보정: 애니메이터 deltaRotation 적용
                characterController.transform.rotation *= _animator.deltaRotation;
                // 6) 중력/점프 속도 추가
                movement += Vector3.up * _verticalSpeed * Time.deltaTime;
                // 7) 캐릭터 컨트롤러로 최종 이동
                characterController.Move(movement);
            }

            // 8) 적 충돌 보정
            var hits = Physics.OverlapCapsule(transform.position, transform.position + Vector3.up * 1.8f, 0.4f, LayerMask.GetMask("Enemy"));
            foreach (Collider hitCol in hits)
            {
                if (Physics.ComputePenetration(
                        _collider, transform.position, transform.rotation,
                        hitCol, hitCol.transform.position, hitCol.transform.rotation,
                        out Vector3 pushDir, out float pushDist))
                {
                    transform.position = Vector3.Lerp(transform.position, transform.position + pushDir * pushDist, 1f);
                }
            }
        }

        bool CanUseManualMove()
        {
            return _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLocomotion
                || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnJog
                || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashLockOnWalk 
                || _currentStateInfo.shortNameHash == PlayerAnimatorConst.hashAirborne;
                   
        }

        Vector3 GetRootMotionMovement()
        {
            Vector3 ret = Vector3.zero;

            RaycastHit hit;
            Ray down = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, Vector3.down);
            if (Physics.Raycast(down, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                ret = Vector3.ProjectOnPlane(_animator.deltaPosition, hit.normal);
            else
                ret = _animator.deltaPosition;

            return ret;
        }

        void Interact()
        {
            if (_interactionController != null)
            {
                _interactionController.Interact();
            }
        }
        
        void Dismentle()
        {
            if (_interactionController != null)
            {
                _interactionController.Interact(true);
            }
        }

        void SwitchMagneticInput()
        {
            if(_weaponHandler.CurrentWeaponType == WeaponType.None)
            {
                return;
            }

            // 쿨다운 중이면 무시
            if (! _canSwitchMagnetic)
                return;

            // 즉시 쿨타임 잠금
            _canSwitchMagnetic = false;

            // 기존 동작
            if (_magneticController != null)
            {
                _magneticController.SwitchMagneticType();
                OnMagneticEffect();
            }

            // 패링 윈도우 열기
            if (_parryWindowCoroutine != null) StopCoroutine(_parryWindowCoroutine);
            _parryWindowActive = true;
            _abilitySystem.SetTag("Parry");
            _parryWindowCoroutine = StartCoroutine(CloseParryWindow());

            // 쿨타임 시작
            if (_switchCooldownCoroutine != null)
                StopCoroutine(_switchCooldownCoroutine);
            _switchCooldownCoroutine = StartCoroutine(SwitchMagneticCooldown());
        }

        // 5초 뒤에 다시 사용 가능하도록 해제
        private IEnumerator SwitchMagneticCooldown()
        {
            yield return new WaitForSecondsRealtime(switchMagneticCooldown);
            _canSwitchMagnetic = true;
            _switchCooldownCoroutine = null;
        }
        
        


        void MagneticPress()
        {
            if (_magneticController != null)
            {
                //*임시 -> 변경 필요
                GameManager.Instance.OnMagneticPressed?.Invoke();
                
                _magneticController.OnPressEnter();
            }
        }
        void MagneticRelease(bool inputValue)
        {
            if (_magneticController == null) return;
            //*임시 -> 변경 필요
            GameManager.Instance.OnMagneticReleased?.Invoke();
            
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
        public void SetCurrentWeapon(WeaponType weaponType, [CanBeNull] MagCore currentMagCore = null)
        {
            _animator.runtimeAnimatorController = animatorControllers[weaponType];
            _weaponHandler.SetCurrentWeapon(weaponType);
            
            //현재 무기 정보 저장 & 이전 무기 드랍
            if (currentMagCore != null)
            {
                if (_weaponHandler.currentMagCore != null) _weaponHandler.DropPrevWeapon(currentMagCore.transform);
                _weaponHandler.currentMagCore = currentMagCore;
            }
            
            canAttack = true;
        }
        
        public void OnMagneticEffect()
        {
            if (_magneticController == null) return;
            var magneticType = _magneticController.GetMagneticType();
            _weaponHandler.ActivateMagnetSwitchEffect(_abilitySystem, magneticType);

            _effect.SwitchPolarity(magneticType, .5f);
        }

        public void UpgradeParts()
        {
            _weaponHandler.UpgradeCurrentParts();
        }
        #endregion
        
        private void RecoverArtifact(CurrentRunData currentRunData)
        {
            foreach (var (key, value) in currentRunData.leftArtifacts)
            {
                _artifactInventory.SetLeftArtifact(key, value).Forget();
            }            
            
            foreach (var (key, value) in currentRunData.rightArtifacts)
            {
                _artifactInventory.SetRightArtifact(key, value).Forget();
            }
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void RespawnStart()
        {
            isInvisible = true;
        }
        
        public void RespawnFinished()
        {
            isDead = false;
            isInvisible = false;
        }

        public void StandFinished()
        {
            _isKnockDown = false;
        }

        public void StartNormalAttack()
        {
            //Force play animation
            _animator.Play(PlayerAnimatorConst.hashEllenCombo1, 0, 0f);
            
            // _animator.SetTrigger(PlayerAnimatorConst.HashMeleeAttack);
            // _animator.SetInteger(PlayerAnimatorConst.HashAttackType, 0);
        }

        public void Death()
        {
            if(isDead) return;
            
            _abilitySystem.TriggerEvent(TriggerEventType.OnDeath, _abilitySystem);
            if(_abilitySystem.GetValue(AttributeType.HP) <= 0)
            {
                isDead = true;
                _animator.SetTrigger(PlayerAnimatorConst.hashDeath);
                GameManager.Instance.ChangeGameState(GameState.GameOver);
            }
        }

        public void Damaged(ExtraData extraData)
        {
            
            
            VFXManager.Instance.TriggerDamageNumberUI(UIManager.Instance.inGameUIController.statusUIController.healthBar.damageTextRectTransform, Vector3.zero, extraData.finalAmount, DAMAGEType.UI_DAMAGE);
            _abilitySystem.TriggerEvent(TriggerEventType.OnDamage, _abilitySystem);
        }

        public void Impulse(ExtraData extraData)
        {
            // 1) 패링 창이 열려 있으면 ─────────
            if (_parryWindowActive)
            {
                if (_parryWindowCoroutine != null)
                    StopCoroutine(_parryWindowCoroutine);
                _abilitySystem.DeleteTag("Parry");
                _parryWindowActive = false;

                // 1-a) 패링 성공 애니 실행
                _animator.SetTrigger(PlayerAnimatorConst.hashParry);
                VFXManager.Instance.TriggerVFX(VFXType.PARRY, transform.position + 0.5f * Vector3.up);
                
                // 1-b) 적을 스턴시키거나 반격 로직 호출
                if (extraData.sourceTransform.TryGetComponent<Enemy>(out var enemy))
                {
                    //넉백?
                    enemy.OnStagger();
                }

                _abilitySystem.TriggerEvent(TriggerEventType.OnParry, _abilitySystem);
                
                if (_parrySlowCoroutine != null)
                    StopCoroutine(_parrySlowCoroutine);
                _parrySlowCoroutine = StartCoroutine(DoParrySlowMotion());

                // (피해는 받지 않음)
                return;
            }

            // 2) 패링 실패 or 타이밍 아웃 시 기존 데미지 처리
            if (isDead || _isKnockDown) return;
            if(extraData.sourceTransform != null)
            {
                // 공격이 들어온 방향
                var dir = extraData.sourceTransform.position - transform.position;
                // 내 캐릭터에 방향에 맞게 계산
                var hurtFrom = transform.InverseTransformDirection(dir.normalized);
                _animator.SetFloat(PlayerAnimatorConst.hashHurtFromX, hurtFrom.x);
                _animator.SetFloat(PlayerAnimatorConst.hashHurtFromY, hurtFrom.z);
                _animator.SetFloat(PlayerAnimatorConst.hashImpulse, _abilitySystem.GetValue(AttributeType.Impulse));
                
                _animator.SetTrigger(PlayerAnimatorConst.hashHurt);
            }
        }

        public (float, bool) GetAttackDamage(float damageMultiplier = 1f)
        {
            bool isCritical = false;
            var damage = _abilitySystem.GetValue(AttributeType.Strength);
            if (Random.value <= _abilitySystem.GetValue(AttributeType.CriticalRate))
            {
                damage *= _abilitySystem.GetValue(AttributeType.CriticalDamage);
                isCritical = true;
            }
            return (damage * damageMultiplier, isCritical);
        }

        IEnumerator CloseParryWindow()
        {
            yield return new WaitForSecondsRealtime(parryWindow);
            _abilitySystem.DeleteTag("Parry");
            _parryWindowActive = false;
            _parryWindowCoroutine = null;
        }
        
        IEnumerator DoParrySlowMotion()
        {
            // (1) 슬로우 시작
            Time.timeScale = parrySlowAmount;
            //Time.fixedDeltaTime = _originalFixedDeltaTime * parrySlowAmount;
            
            // (2) real-time 대기
            yield return new WaitForSecondsRealtime(parrySlowDuration);

            // (3) 복원
            Time.timeScale = 1f;
            //Time.fixedDeltaTime = _originalFixedDeltaTime;

            _parrySlowCoroutine = null;
        }

        public void OnKnockDown()
        {
            _isKnockDown = true;
        }

        void MoveSpeedChanged()
        {
            _animator.SetFloat(PlayerAnimatorConst.hashMoveSpeed, _abilitySystem.GetValue(AttributeType.MoveSpeed));
            _maxForwardSpeed = k_maxForwardSpeed * _abilitySystem.GetValue(AttributeType.MoveSpeed);
        }

        void AttackSpeedChanged()
        {
            _animator.SetFloat(PlayerAnimatorConst.hashAttackSpeed, _abilitySystem.GetValue(AttributeType.AttackSpeed));
        }

        void OnAnimatorIK(int layerIndex)
        {
            if(_lockOnSystem.IsLockOn)
            {
                Transform target = _lockOnSystem.currentTarget;
                _animator.SetLookAtPosition(target.position + Vector3.up * 1.5f);
                _animator.SetLookAtWeight(0.6f);
            }
        }
        
        void PerformDodge()
        {
            //_inCombo = false;
            _isDodging = true;  // ★ 회피 시작 플래그 켜기
            Vector2 moveInput = _inputHandler.ForceMoveInput;
            _animator.SetFloat(PlayerAnimatorConst.hashDodgeX, moveInput.x);
            _animator.SetFloat(PlayerAnimatorConst.hashDodgeY, moveInput.y);
            _animator.SetTrigger(PlayerAnimatorConst.hashDodge);
            
            SetForceRotation();

            Quaternion dodgeRotation = transform.rotation;
            if(_lockOnSystem.IsLockOn)
            {
                Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
                if (dir.sqrMagnitude > 0.001f)
                {
                    dodgeRotation = Quaternion.LookRotation(dir);
                }        
            }

            VFXManager.Instance.TriggerVFX(VFXType.DODGE_DUST, transform.position, dodgeRotation);

            StartCoroutine(UnblockAfterDodge());
        }

        IEnumerator UnblockAfterDodge()
        {
            yield return new WaitForSeconds(0.8f);
            _isDodging = false;   // ★ 회피 끝났으니 다시 허용
            _animator.SetFloat(PlayerAnimatorConst.hashDodgeX, 0f);
            _animator.SetFloat(PlayerAnimatorConst.hashDodgeY, 0f);
        }


    }
}