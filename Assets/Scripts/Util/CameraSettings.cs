using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Moon
{
    public class CameraSettings : MonoBehaviour
    {
        public enum InputChoice
        {
            KeyboardAndMouse, Controller, LockOn, 
        }

        [Serializable]
        public struct InvertSettings
        {
            public bool invertX;
            public bool invertY;
        }


        public Transform follow;
        public Transform lookAt;
        public CinemachineFreeLook keyboardAndMouseCamera;
        public CinemachineFreeLook controllerCamera;
        public CinemachineVirtualCamera interactionCamera;
        public CinemachineVirtualCamera lockOnCamera;
        public InputChoice inputChoice;
        public InvertSettings keyboardAndMouseInvertSettings;
        public InvertSettings controllerInvertSettings;
        public bool allowRuntimeCameraSettingsChanges;

        public CinemachineFreeLook Current
        {
            get { return inputChoice == InputChoice.KeyboardAndMouse ? keyboardAndMouseCamera : controllerCamera; }
        }

        void Reset()
        {
            VolumeController.Initialize();
            CinemachineImpulseController.Initialize();

            Transform keyboardAndMouseCameraTransform = transform.Find("KeyboardAndMouseFreeLookRig");
            if (keyboardAndMouseCameraTransform != null)
                keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineFreeLook>();

            Transform controllerCameraTransform = transform.Find("ControllerFreeLookRig");
            if (controllerCameraTransform != null)
                controllerCamera = controllerCameraTransform.GetComponent<CinemachineFreeLook>();

            Transform interactionCameraTransform = transform.Find("InteractionCamera");
            if (interactionCameraTransform != null)
                interactionCamera = interactionCameraTransform.GetComponent<CinemachineVirtualCamera>();

            Transform lockOnCameraTransform = transform.Find("LockOnCamera");
            if (lockOnCameraTransform != null)
                lockOnCamera = lockOnCameraTransform.GetComponent<CinemachineVirtualCamera>();

            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.name == "Player")
            {
                follow = playerController.transform;

                lookAt = follow.Find("HeadTarget");

                if (playerController.cameraSettings == null)
                    playerController.cameraSettings = this;
            }

            Current.m_XAxis.m_InputAxisName = "";
            Current.m_YAxis.m_InputAxisName = "";
        }

        void Awake()
        {
            Reset();
            UpdateCameraSettings();
        }

        void Update()
        {
            if (allowRuntimeCameraSettingsChanges)
            {
                UpdateCameraSettings();
            }
        }

        void UpdateCameraSettings()
        {
            keyboardAndMouseCamera.Follow = follow;
            keyboardAndMouseCamera.LookAt = lookAt;
            keyboardAndMouseCamera.m_XAxis.m_InvertInput = keyboardAndMouseInvertSettings.invertX;
            keyboardAndMouseCamera.m_YAxis.m_InvertInput = keyboardAndMouseInvertSettings.invertY;

            controllerCamera.m_XAxis.m_InvertInput = controllerInvertSettings.invertX;
            controllerCamera.m_YAxis.m_InvertInput = controllerInvertSettings.invertY;
            controllerCamera.Follow = follow;
            controllerCamera.LookAt = lookAt;

            keyboardAndMouseCamera.Priority = inputChoice == InputChoice.KeyboardAndMouse ? 1 : 0;
            controllerCamera.Priority = inputChoice == InputChoice.Controller ? 1 : 0;
        }

        public void EnableCameraMove()
        {
            if(keyboardAndMouseCamera != null)
            {
                keyboardAndMouseCamera.GetComponent<CinemachineInputProvider>().enabled = true;          
            }

            if(controllerCamera != null)
            {
                controllerCamera.GetComponent<CinemachineInputProvider>().enabled = true;          
            }
        }

        public void DisableCameraMove()
        {
            Current.m_XAxis.m_InputAxisName = "";
            Current.m_YAxis.m_InputAxisName = "";

            if(keyboardAndMouseCamera != null)
            {
                keyboardAndMouseCamera.GetComponent<CinemachineInputProvider>().enabled = false;          
            }

            if(controllerCamera != null)
            {
                controllerCamera.GetComponent<CinemachineInputProvider>().enabled = false;          
            }
        }

        public IEnumerator AdjustFOV(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Current.m_Lens.FieldOfView = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            Current.m_Lens.FieldOfView = to;
        }

        public IEnumerator RestoreFOV(float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Current.m_Lens.FieldOfView = Mathf.Lerp(Current.m_Lens.FieldOfView, to, t / duration);
                yield return null;
            }
            Current.m_Lens.FieldOfView = to;
        }

        public void SetCinemachineColliderEnabled(bool isEnabled)
        {
            var collider = Current.GetComponent<CinemachineCollider>();

            if (collider != null)
            {
                collider.enabled = isEnabled;
            }
        }
    } 
}
