using System;
using System.Collections;
using System.Collections.Generic;

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
        bool _run;
        bool _jump;
        bool _attack1;
        bool _attack2;
        bool _interact;

        //다른 이유로 조작이 불가능한 경우 사용하는 변수
        bool _externalInputBlocked;

        public Vector2 MoveInput
        {
            get
            {
                if(IsContollerInputBlocked())
                    return Vector2.zero;
                return _movement;
            }
        }
        public Vector2 CameraInput
        {
            get
            {
                if(IsContollerInputBlocked())
                    return Vector2.zero;
                return _cameraMovement;
            }
        }
        public bool RunInput
        {
            get { return _run && !IsContollerInputBlocked(); }
        }
        public bool JumpInput
        {
            get { return _jump && !IsContollerInputBlocked(); }
        }

        public bool Attack1
        {
            get { return _attack1 && !IsContollerInputBlocked(); }
        }

        public bool Attack2
        {
            get { return _attack2 && !IsContollerInputBlocked(); }
        }
        public bool InteractInput
        {
            get { return _interact && !IsContollerInputBlocked(); }
        }

        WaitForSeconds _attackInputWait;
        WaitForSeconds _inputWait;
        Coroutine _attack1WaitCoroutine;
        Coroutine _attack2WaitCoroutine;
        Coroutine _interactWaitCoroutine;
        const float _AttackInputDuration = 0.03f;
        const float _inputDuration = 0.1f;

        void Start()
        {
            _attackInputWait = new WaitForSeconds(_AttackInputDuration);
            _inputWait = new WaitForSeconds(_inputDuration);

            playerInput = GetComponent<PlayerInput>();
            playerInput.actions["Move"].performed += ctx => _movement = ctx.ReadValue<Vector2>();
            playerInput.actions["Move"].canceled += ctx => _movement = Vector2.zero;

            playerInput.actions["Look"].performed += ctx => _cameraMovement = ctx.ReadValue<Vector2>();
            playerInput.actions["Look"].canceled += ctx => _cameraMovement = Vector2.zero;

            playerInput.actions["Jump"].performed += ctx => _jump = true;
            playerInput.actions["Jump"].canceled += ctx => _jump = false;

            playerInput.actions["Attack1"].performed += ctx => {
                if (_attack1WaitCoroutine != null)
                    StopCoroutine(_attack1WaitCoroutine);

                _attack1WaitCoroutine = StartCoroutine(Attack1Wait());
            };
            playerInput.actions["Attack2"].performed += ctx => {
                if (_attack2WaitCoroutine != null)
                    StopCoroutine(_attack2WaitCoroutine);

                _attack2WaitCoroutine = StartCoroutine(Attack2Wait());
            };

            playerInput.actions["Sprint"].performed += ctx => _run = true;
            playerInput.actions["Sprint"].canceled += ctx => _run = false;

            
            playerInput.actions["Interact"].performed += ctx => _interact = true;
            playerInput.actions["Interact"].canceled += ctx => _interact = false;

            //Test Cursor disable
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        
        public bool IsContollerInputBlocked()
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
            CameraShake.Shake(0.2f, 0.2f);
            yield return _attackInputWait;

            _attack2 = false;
        }

        IEnumerator InteractWait()
        {
            Debug.Log("InteractWait Start");
            _interact = true;

            yield return _inputWait;

            _interact = false;
        }


        public bool HaveControl()
        {
            return !_externalInputBlocked;
        }

        public void ReleaseControl()
        {
            _externalInputBlocked = true;
        }

        public void GainControl()
        {
            _externalInputBlocked = false;
        }
    }
}