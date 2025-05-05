using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using hvvan;
using UnityEngine;

public class MagneticCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCam;

    private CinemachineVirtualCamera _vcam;
    private CinemachinePOV _pov;
    private Camera _mainCamera;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();
        _mainCamera = Camera.main;
        
        GameManager.Instance.OnMagneticPressed += OnMagneticPressed;
        GameManager.Instance.OnMagneticReleased += OnMagneticReleased;
    }

    private void LateUpdate()
    {
        // POV가 없을 때만 FreeLook을 따라감
        if (_pov == null)
        {
            _vcam.Follow = freeLookCam.Follow;
            
            var state = freeLookCam.State;
            _vcam.transform.position = state.FinalPosition;
            _vcam.transform.rotation = state.FinalOrientation;
        }
    }

    private void OnMagneticPressed()
    {
        // POV 컴포넌트 추가 전 먼저 우선순위 올림
        _vcam.Priority = 100;
        
        // 잠깐 대기 (이전 프레임의 상태가 반영되도록)
        StartCoroutine(DelayedPOVSetup());
    }   

    private IEnumerator DelayedPOVSetup()
    {
        yield return new WaitForEndOfFrame();
        
        // POV 추가
        _pov = _vcam.AddCinemachineComponent<CinemachinePOV>();
        
        // 현재 카메라 회전 기준으로 POV 초기화
        Vector3 currentRotation = _mainCamera.transform.eulerAngles;
        float xAngle = currentRotation.x;
        
        // 360도 -> -180~180도 변환
        if (xAngle > 180) xAngle -= 360;
        
        _pov.m_HorizontalAxis.Value = currentRotation.y;
        _pov.m_VerticalAxis.Value = xAngle;
    }

    private void OnMagneticReleased()
    {
        _vcam.Priority = 0;
        
        // POV 컴포넌트 제거
        if (_pov != null)
        {
            _vcam.DestroyCinemachineComponent<CinemachinePOV>();
            _pov = null;
        }
    }
}