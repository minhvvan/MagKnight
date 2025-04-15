using UnityEngine;

namespace Moon
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IInteractor
    {
        CharacterController _characterController;
        Animator _animator;
        InputHandler _inputHandler;
        [SerializeField] private InteractionController interactionController;
        [SerializeField] private WeaponHandler weaponHandler;

        [SerializeField] public float maxForwardSpeed = 8f;        
        [SerializeField] public float gravity = 20f;               
        [SerializeField] public float jumpSpeed = 10f;             
        [SerializeField] public float minTurnSpeed = 400f;         
        [SerializeField] public float maxTurnSpeed = 1200f;        
        [SerializeField] public float idleTimeout = 5f;            
        [SerializeField] public bool canAttack;
        

         public CameraSettings cameraSettings;

        protected AnimatorStateInfo _currentStateInfo;    // Information about the base layer of the animator cached.
        protected AnimatorStateInfo _nextStateInfo;
        protected bool _isAnimatorTransitioning;
        protected AnimatorStateInfo _previousCurrentStateInfo;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo _previousNextStateInfo;
        protected bool _previousIsAnimatorTransitioning;
        protected bool _isGrounded = true;            // Whether or not Ellen is currently standing on the ground.
        protected bool _previouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.
        protected bool _readyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.
        protected float _desiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.
        protected float _forwardSpeed;                // How fast Ellen is currently going along the ground.
        protected float _verticalSpeed;               // How fast Ellen is currently moving up or down.
        
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

        // States
        readonly int _HashLocomotion = Animator.StringToHash("Locomotion");
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
        readonly int _HashEllenDeath = Animator.StringToHash("EllenDeath");

        // Tags
        readonly int _HashBlockInput = Animator.StringToHash("BlockInput");

        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(_inputHandler.MoveInput.sqrMagnitude, 0f); }
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

        void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
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
            }

            CalculateForwardMovement();
            CalculateVerticalMovement();

            SetTargetRotation();

            if (IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();

            //PlayAudio();

            TimeoutToIdle();

            _previouslyGrounded = _isGrounded;
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
            }
            else
            {
                cameraSettings.EnableCameraMove();
            }
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
            // Create three variables, move input local to the player, flattened forward direction of the camera and a local target rotation.
            Vector2 moveInput = _inputHandler.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            
            Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;
            
            // If the local movement direction is the opposite of forward then the target rotation should be towards the camera.
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                // Otherwise the rotation should be the offset of the input from the camera's forward.
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            // The desired forward direction of Ellen.
            Vector3 resultingForward = targetRotation * Vector3.forward;

//가까운 적 관련 회전 루틴 - 참고 후 삭제
#if false
            // If attacking try to orient to close enemies.
            if (_InAttack)
            {
                // Find all the enemies in the local area.
                Vector3 centre = transform.position + transform.forward * 2.0f + transform.up;
                Vector3 halfExtents = new Vector3(3.0f, 1.0f, 2.0f);
                int layerMask = 1 << LayerMask.NameToLayer("Enemy");
                int count = Physics.OverlapBoxNonAlloc(centre, halfExtents, _OverlapResult, targetRotation, layerMask);

                // Go through all the enemies in the local area...
                float closestDot = 0.0f;
                Vector3 closestForward = Vector3.zero;
                int closest = -1;

                for (int i = 0; i < count; ++i)
                {
                    // ... and for each get a vector from the player to the enemy.
                    Vector3 playerToEnemy = _OverlapResult[i].transform.position - transform.position;
                    playerToEnemy.y = 0;
                    playerToEnemy.Normalize();

                    // Find the dot product between the direction the player wants to go and the direction to the enemy.
                    // This will be larger the closer to Ellen's desired direction the direction to the enemy is.
                    float d = Vector3.Dot(resultingForward, playerToEnemy);

                    // Store the closest enemy.
                    if (d > k_MinEnemyDotCoeff && d > closestDot)
                    {
                        closestForward = playerToEnemy;
                        closestDot = d;
                        closest = i;
                    }
                }

                // If there is a close enemy...
                if (closest != -1)
                {
                    // The desired forward is the direction to the closest enemy.
                    resultingForward = closestForward;
                    
                    // We also directly set the rotation, as we want snappy fight and orientation isn't updated in the UpdateOrientation function during an atatck.
                    transform.rotation = Quaternion.LookRotation(resultingForward);
                }
            }
#endif
            // Find the difference between the current rotation of the player and the desired rotation of the player in radians.
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            _angleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            _targetRotation = targetRotation;
        }

        // Called each physics step to help determine whether Ellen can turn under player input.
        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLocomotion || _nextStateInfo.shortNameHash == _HashLocomotion;
            bool updateOrientationForAirborne = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashAirborne || _nextStateInfo.shortNameHash == _HashAirborne;
            bool updateOrientationForLanding = !_isAnimatorTransitioning && _currentStateInfo.shortNameHash == _HashLanding || _nextStateInfo.shortNameHash == _HashLanding;

            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || _inCombo && !_inAttack;
        }

        
        void UpdateOrientation()
        {
            _animator.SetFloat(_HashAngleDeltaRad, _angleDiff * Mathf.Deg2Rad);

            if(_currentStateInfo.shortNameHash == _HashLocomotion)
            {
                Vector3 localInput = new Vector3(_inputHandler.MoveInput.x, 0f, _inputHandler.MoveInput.y);
                float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, _forwardSpeed / _desiredForwardSpeed);
                float actualTurnSpeed = _isGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
                
                _targetRotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, actualTurnSpeed * Time.deltaTime);

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
            Vector3 movement;

            if (_isGrounded)
            {
                if(_currentStateInfo.shortNameHash == _HashLocomotion)
                {
                    movement = _forwardSpeed * transform.forward * Time.deltaTime;
                }
                else
                {                        
                    RaycastHit hit;
                    Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                    if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    {
                        // ... and get the movement of the root motion rotated to lie along the plane of the ground.
                        movement = Vector3.ProjectOnPlane(_animator.deltaPosition, hit.normal);
                    }
                    else
                    {
                        movement = _animator.deltaPosition;
                    }
                }
            }
            else
            {
                // If not grounded the movement is just in the forward direction.
                movement = _forwardSpeed * transform.forward * Time.deltaTime;
            }
            
            _characterController.transform.rotation *= _animator.deltaRotation;
            
            movement += _verticalSpeed * Vector3.up * Time.deltaTime;
            _characterController.Move(movement);
            _isGrounded = _characterController.isGrounded;
            
            if (!_isGrounded)
                _animator.SetFloat(_HashAirborneVerticalSpeed, _verticalSpeed);

            
            _animator.SetBool(_HashGrounded, _isGrounded);
        }
        

        void Interact()
        {
            if (interactionController != null)
            {
                interactionController.Interact();
            }
        }

#if false //애니메이션 이벤트로 데미지처리 관련 활성화/비활성화
        // This is called by an animation event when Ellen swings her staff.
        public void MeleeAttackStart(int throwing = 0)
        {
            meleeWeapon.BeginAttack(throwing != 0);
            _InAttack = true;
        }

        // This is called by an animation event when Ellen finishes swinging her staff.
        public void MeleeAttackEnd()
        {
            meleeWeapon.EndAttack();
            _InAttack = false;
        }
#endif

        #region Weapon
        public void SetCurrentWeapon(WeaponType weaponType)
        {
            weaponHandler.SetCurrentWeapon(weaponType);
        }
        #endregion
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}