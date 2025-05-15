using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using hvvan;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Moon
{
    public class InputHandler : MonoBehaviour
    {
        [NonSerialized] public bool playerControllerInputBlocked;

        //new input system
        PlayerInput playerInput;
        
        /// Player Input
        Vector2 _movement;
        Vector2 _cameraMovement;
        bool _jump;
        bool _attack1;
        bool _attack2;
        bool _lockOn;
        bool _dodge;
        bool _nextTarget;
        bool _interact;
        bool _magnetic;
        bool _magneticSecond;
        bool _switchMangetic;
        bool _skill;
        
        //다른 이유로 조작이 불가능한 경우 사용하는 변수
        bool _externalInputBlocked;

        public Vector2 MoveInput
        {
            get
            {
                if(IsControllerInputBlocked())
                    return Vector2.zero;
                return _movement;
            }
        }

        public Vector2 ForceMoveInput
        {
            get
            {
                return _movement;
            }
        }

        public Vector2 CameraInput
        {
            get
            {
                if(IsControllerInputBlocked())
                    return Vector2.zero;
                return _cameraMovement;
            }
        }

        public bool JumpInput
        {
            get { return _jump && !IsControllerInputBlocked(); }
        }

        public bool Attack1
        {
            get { return _attack1 && !IsControllerInputBlocked(); }
        }

        public bool Attack2
        {
            get { return _attack2 && !IsControllerInputBlocked(); }
        }

        public bool LockOnInput
        {
            get
            {
                bool ret = _lockOn && !IsControllerInputBlocked();
                _lockOn = false;
                return ret;    
            }
        }
        public bool InteractInput
        {
            get { 
                bool ret = _interact && !IsControllerInputBlocked();
                _interact = false;
                return ret;    
            }
        }

        public bool SkillInput
        {
            get
            {
                bool ret = _skill && !IsControllerInputBlocked();
                _skill = false;
                return ret;    
            }
        }

        public bool DodgeInput
        {
            get 
            { 
                bool ret = _dodge && !IsControllerInputBlocked();
                _dodge = false;
                return ret;
            }
        }
        
        public Action SwitchMagneticInput;
        public Action magneticInput;
        public Action<bool> magneticOutput;

        WaitForSeconds _attackInputWait;
        WaitForSeconds _inputWait;
        Coroutine _attack1WaitCoroutine;
        Coroutine _attack2WaitCoroutine;
        Coroutine _lockOnWaitCoroutine;
        Coroutine _interactWaitCoroutine;
        Coroutine _magneticWaitCoroutine;
        const float _AttackInputDuration = 0.03f;
        const float _inputDuration = 0.1f;
        const float _magneticInputDuration = 0.15f;

        /// Input Action Callbacks
        Action<InputAction.CallbackContext> _pressAttack1Callback;
        Action<InputAction.CallbackContext> _releaseAttack1Callback;
        Action<InputAction.CallbackContext> _pressAttack2Callback;
        Action<InputAction.CallbackContext> _releaseAttack2Callback;
        Action<InputAction.CallbackContext> _pressMagneticCallback;
        Action<InputAction.CallbackContext> _releaseMagneticCallback;
        Action<InputAction.CallbackContext> _pressSwitchMagneticCallback;
        Action<InputAction.CallbackContext> _releaseSwitchMagneticCallback;
        Action<InputAction.CallbackContext> _pressMoveCallback;
        Action<InputAction.CallbackContext> _releaseMoveCallback;
        Action<InputAction.CallbackContext> _pressCameraCallback;
        Action<InputAction.CallbackContext> _releaseCameraCallback;
        Action<InputAction.CallbackContext> _pressJumpCallback;
        Action<InputAction.CallbackContext> _releaseJumpCallback;
        Action<InputAction.CallbackContext> _pressInteractCallback;
        Action<InputAction.CallbackContext> _releaseInteractCallback;
        Action<InputAction.CallbackContext> _pressPauseCallback;
        Action<InputAction.CallbackContext> _releasePauseCallback;
        Action<InputAction.CallbackContext> _pressLockOnCallback;
        Action<InputAction.CallbackContext> _releaseLockOnCallback;
        Action<InputAction.CallbackContext> _pressSkillOnCallback;
        Action<InputAction.CallbackContext> _releaseSkillOnCallback;
        Action<InputAction.CallbackContext> _pressDodgeCallback;
        Action<InputAction.CallbackContext> _releaseDodgeCallback;
        
        private CancellationTokenSource _controlCts;

        void Start()
        {
            _pressAttack1Callback = ctx => PressAttack1Input(ctx);
            _pressAttack2Callback = ctx => PressAttack2Input(ctx);
            _pressMagneticCallback = ctx => PressMagneticInput(ctx);
            _pressSwitchMagneticCallback = ctx => PressSwitchMagneticInput(ctx);
            _pressMoveCallback = ctx => PressMoveInput(ctx);
            _pressCameraCallback = ctx => PressCameraInput(ctx);
            _pressJumpCallback = ctx => PressJumpInput(ctx);
            _pressInteractCallback = ctx => PressInteractInput(ctx);
            _pressPauseCallback = ctx => PressPauseInput(ctx);
            _pressLockOnCallback   = ctx => PressLockOnInput(ctx);
            _pressSkillOnCallback = ctx => PressSkillInput(ctx);
            _pressDodgeCallback = ctx => PressDodgeInput(ctx);

            _releaseAttack1Callback = ctx => ReleaseAttack1Input(ctx);
            _releaseAttack2Callback = ctx => ReleaseAttack2Input(ctx);
            _releaseMagneticCallback = ctx => ReleaseMagneticInput(ctx);
            _releaseSwitchMagneticCallback = ctx => ReleaseSwitchMagneticInput(ctx);
            _releaseMoveCallback = ctx => ReleaseMoveInput(ctx);
            _releaseCameraCallback = ctx => ReleaseCameraInput(ctx);
            _releaseJumpCallback = ctx => ReleaseJumpInput(ctx);
            _releaseInteractCallback = ctx => ReleaseInteractInput(ctx);
            _releasePauseCallback = ctx => ReleasePauseInput(ctx);
            _releaseLockOnCallback   = ctx => ReleaseLockOnInput(ctx);
            _releaseSkillOnCallback = ctx => ReleaseSkillInput(ctx);
            _releaseDodgeCallback   = ctx => ReleaseDodgeInput(ctx);




            _attackInputWait = new WaitForSeconds(_AttackInputDuration);
            _inputWait = new WaitForSeconds(_inputDuration);

            playerInput = GetComponent<PlayerInput>();

            playerInput.actions["Move"].performed += _pressMoveCallback;
            playerInput.actions["Move"].canceled += _releaseMoveCallback;

            playerInput.actions["Look"].performed += _pressCameraCallback;
            playerInput.actions["Look"].canceled += _releaseCameraCallback;

            playerInput.actions["Jump"].performed += _pressJumpCallback;
            playerInput.actions["Jump"].canceled += _releaseJumpCallback;

            playerInput.actions["Attack1"].performed += _pressAttack1Callback;
            playerInput.actions["Attack2"].performed += _pressAttack2Callback;

            playerInput.actions["LockOn"].performed     += _pressLockOnCallback;
            playerInput.actions["LockOn"].canceled += _releaseLockOnCallback;
            
            playerInput.actions["Dodge"].performed += _pressDodgeCallback;
            playerInput.actions["Dodge"].canceled += _releaseDodgeCallback;
            
            playerInput.actions["Interact"].performed += _pressInteractCallback;
            playerInput.actions["Interact"].canceled += _releaseInteractCallback;

            playerInput.actions["Magnetic"].performed += _pressMagneticCallback;
            playerInput.actions["Magnetic"].canceled += _releaseMagneticCallback;

            playerInput.actions["SwitchMagnetic"].performed += _pressSwitchMagneticCallback;
            playerInput.actions["SwitchMagnetic"].canceled += _releaseSwitchMagneticCallback;
            
            playerInput.actions["Pause"].performed += _pressPauseCallback;
            playerInput.actions["Pause"].canceled += _releasePauseCallback;

            playerInput.actions["Skill"].performed += _pressSkillOnCallback;
            playerInput.actions["Skill"].canceled += _releaseSkillOnCallback;
            
            UIManager.Instance.DisableCursor();
            InteractionEvent.OnDialogueStart += ReleaseControl;
            InteractionEvent.OnDialogueEnd += GainControlDelayed;
        }

        void OnDestroy()
        {
            playerInput.actions["Move"].performed -= _pressMoveCallback;
            playerInput.actions["Move"].canceled -= _releaseMoveCallback;

            playerInput.actions["Look"].performed -= _pressCameraCallback;
            playerInput.actions["Look"].canceled -= _releaseCameraCallback;

            playerInput.actions["Jump"].performed -= _pressJumpCallback;
            playerInput.actions["Jump"].canceled -= _releaseJumpCallback;

            playerInput.actions["Attack1"].performed -= _pressAttack1Callback;
            playerInput.actions["Attack2"].performed -= _pressAttack2Callback;

            playerInput.actions["Interact"].performed -= _pressInteractCallback;
            playerInput.actions["Interact"].canceled -= _releaseInteractCallback;

            playerInput.actions["Magnetic"].performed -= _pressMagneticCallback;
            playerInput.actions["Magnetic"].canceled -= _releaseMagneticCallback;

            playerInput.actions["SwitchMagnetic"].performed -= _pressSwitchMagneticCallback;
            playerInput.actions["SwitchMagnetic"].canceled -= _releaseSwitchMagneticCallback;
            
            playerInput.actions["LockOn"].performed -= _pressLockOnCallback;
            playerInput.actions["LockOn"].canceled -= _releaseLockOnCallback;
            
            playerInput.actions["Dodge"].performed -= _pressDodgeCallback;
            playerInput.actions["Dodge"].canceled -= _releaseDodgeCallback;

            playerInput.actions["Skill"].performed -= _pressSkillOnCallback;
            playerInput.actions["Skill"].canceled -= _releaseSkillOnCallback;

            playerInput.actions["Pause"].performed -= _pressPauseCallback;
            playerInput.actions["Pause"].canceled -= _releasePauseCallback;
            
            InteractionEvent.OnDialogueStart -= ReleaseControl;
            InteractionEvent.OnDialogueEnd -= GainControlDelayed;
        }

        void PressMoveInput(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        void ReleaseMoveInput(InputAction.CallbackContext context)
        {
            _movement = Vector2.zero;
        }

        void PressCameraInput(InputAction.CallbackContext context)
        {
            _cameraMovement = context.ReadValue<Vector2>();
        }

        void ReleaseCameraInput(InputAction.CallbackContext context)
        {
            _cameraMovement = Vector2.zero;
        }

        void PressJumpInput(InputAction.CallbackContext context)
        {
            _jump = true;
        }

        void ReleaseJumpInput(InputAction.CallbackContext context)
        {
            _jump = false;
        }
        void PressAttack1Input(InputAction.CallbackContext context)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            else
            {
                UIManager.Instance.DisableCursor();
            }

            if (_attack1WaitCoroutine != null)
                StopCoroutine(_attack1WaitCoroutine);

            _attack1WaitCoroutine = StartCoroutine(Attack1Wait());
        }

        void ReleaseAttack1Input(InputAction.CallbackContext context)
        {
        }

        void PressAttack2Input(InputAction.CallbackContext context)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            else
            {
                UIManager.Instance.DisableCursor();
            }

            if (_attack2WaitCoroutine != null)
                StopCoroutine(_attack2WaitCoroutine);

            _attack2WaitCoroutine = StartCoroutine(Attack2Wait());
        }

        void ReleaseAttack2Input(InputAction.CallbackContext context)
        {
        }

        void PressLockOnInput(InputAction.CallbackContext ctx)
        {
            _lockOn = true;
        }
        void ReleaseLockOnInput(InputAction.CallbackContext ctx)
        {
            _lockOn = false;
        }

        void PressDodgeInput(InputAction.CallbackContext context)
        {
            _dodge = true;
        }
        
        void ReleaseDodgeInput(InputAction.CallbackContext context)
        {
            _dodge = false;
        }
        void PressInteractInput(InputAction.CallbackContext context)
        {
            _interact = true;        
        }

        void ReleaseInteractInput(InputAction.CallbackContext context)
        {
            _interact = false;
        }

        void PressMagneticInput(InputAction.CallbackContext context)
        {
            if(Time.timeScale == 0) return;

            _magnetic = true;
            magneticInput?.Invoke();
            if(_magneticWaitCoroutine != null) StopCoroutine(_magneticWaitCoroutine);
            _magneticWaitCoroutine = StartCoroutine(MagneticWait());
        }

        void ReleaseMagneticInput(InputAction.CallbackContext context)
        {
            if(Time.timeScale == 0) return;

            _magnetic = false;
            magneticOutput?.Invoke(_magneticSecond);
            _magneticSecond = false;
        }

        void PressSwitchMagneticInput(InputAction.CallbackContext context)
        {
            if(Time.timeScale == 0) return;

            _switchMangetic = true;
            SwitchMagneticInput?.Invoke();
        }

        void ReleaseSwitchMagneticInput(InputAction.CallbackContext context)
        {
            if(Time.timeScale == 0) return;
            
            _switchMangetic = false;
            //SwitchMangeticInput?.Invoke(); //Press시에만 호출 필요
        }
        
        private void ReleasePauseInput(InputAction.CallbackContext ctx)
        {
            
        }

        private void PressPauseInput(InputAction.CallbackContext ctx)
        {
            if(ScreenFader.IsFading) return;

            if(UIManager.Instance.popupUIController.IsAnyPopupUIActive())
            {
                if(UIManager.Instance.popupUIController != null)
                {
                    UIManager.Instance.popupUIController.HideAllPopupAvailableUI();
                    UIManager.Instance.DisableCursor();
                }
            }
            else
            {
                UIManager.Instance.ShowPauseMenuUI();
            }
        }

        void PressSkillInput(InputAction.CallbackContext ctx)
        {
            _skill = true;
        }

        void ReleaseSkillInput(InputAction.CallbackContext ctx)
        {
            _skill = false;
        }
        
        public bool IsControllerInputBlocked()
        {
            return playerControllerInputBlocked || _externalInputBlocked;
        }

        IEnumerator Attack1Wait()
        {
            _attack1 = true;

            yield return _attackInputWait;

            _attack1 = false;
        }

        IEnumerator Attack2Wait()
        {
            _attack2 = true;

            //Test Code - Camera Shake
            //CameraShake.Shake(0.2f, 0.2f);
            yield return _attackInputWait;

            _attack2 = false;
        }

        IEnumerator MagneticWait()
        {
            var elapsedTime = 0f;
            while (elapsedTime < _magneticInputDuration)
            {
                if(!_magnetic) yield break;
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            _magneticSecond = true;
        }

        public bool HaveControl()
        {
            return !_externalInputBlocked;
        }

        public void ReleaseControl()
        {
            CancelPendingControlGain();
            _externalInputBlocked = true;
        }

        public void GainControl()
        {
            CancelPendingControlGain();
            _externalInputBlocked = false;
        }

        void GainControlDelayed()
        {
            _controlCts = new CancellationTokenSource();
            
            UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _controlCts.Token).ContinueWith(() =>
            {
                if (!_controlCts.Token.IsCancellationRequested)
                {
                    _externalInputBlocked = false;
                }
            });
        }
        
        private void CancelPendingControlGain()
        {
            if (_controlCts != null)
            {
                _controlCts.Cancel();
                _controlCts.Dispose();
                _controlCts = null;
            }
        }
    }
}