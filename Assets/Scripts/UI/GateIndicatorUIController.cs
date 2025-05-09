using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateIndicatorUIController : MonoBehaviour
{
    private List<Gate> _gates = new List<Gate>();
    public List<RectTransform> indicator = new List<RectTransform>();
    public Camera mainCamera;
    
    [SerializeField] private Canvas mainCanvas;
    
    [Header("Scale Settings")]
    [SerializeField] private float baseDistance = 10f; // 기준 거리
    [SerializeField] private float minScale = 0.5f; // 최소 스케일
    [SerializeField] private float maxScale = 2.0f; // 최대 스케일

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (loadSceneMode == LoadSceneMode.Additive) return;
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (_gates == null || indicator == null) return;
        if(_gates.Count == 0) return;

        for (int i = 0; i < _gates.Count; i++)
        {
            if(!_gates[i].gameObject.activeInHierarchy) continue;
            
            if (i >= indicator.Count) continue;
            
            indicator[i].gameObject.SetActive(true);
            
            // 게이트와의 거리 계산
            float distanceToGate = Vector3.Distance(mainCamera.transform.position, _gates[i].indicatorPoint.position);
            
            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(_gates[i].indicatorPoint.position);
        
            // 오브젝트가 카메라 뒤에 있을 경우 처리
            if (screenPos.z < 0)
            {
                indicator[i].gameObject.SetActive(false);
                continue;
            }
        
            // 스크린 좌표를 캔버스 좌표로 변환
            indicator[i].position = screenPos;
            
            // 거리에 따른 스케일 조정 - 간단하게 적용
            AdjustScale(indicator[i], distanceToGate);
        }
    }
    
    // 간단한 스케일 조정 함수
    private void AdjustScale(RectTransform indicatorRect, float distance)
    {
        // 거리에 반비례하는 스케일 계산 (가까울수록 작게, 멀수록 크게)
        float scale = baseDistance / Mathf.Max(distance, 0.1f);
        
        // 스케일 범위 제한
        scale = Mathf.Clamp(scale, minScale, maxScale);
        
        // 모든 인디케이터 스케일이 동일하므로 (1,1,1) 기준으로 적용
        indicatorRect.localScale = new Vector3(scale, scale, scale);
    }

    public void BindGate(Gate gate)
    {
        _gates.Add(gate);
    }

    public void UnBindGate(Gate gate)
    {
        if (!_gates.Contains(gate)) return;
        
        int index = _gates.IndexOf(gate);
        _gates.Remove(gate);
        
        if (index >= 0 && index < indicator.Count)
        {
            indicator[index].gameObject.SetActive(false);
        }
    }
}