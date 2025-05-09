using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateIndicatorUI : MonoBehaviour
{
    private List<Gate> _gates = new List<Gate>();
    public List<RectTransform> indicator = new List<RectTransform>();
    public Camera mainCamera;
    
    [SerializeField] private Canvas mainCanvas;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
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
            indicator[i].gameObject.SetActive(true);
            
            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(_gates[i].indicatorPoint.position);
        
            // 오브젝트가 카메라 뒤에 있을 경우 처리
            if (screenPos.z < 0)
            {
                indicator[i].gameObject.SetActive(false);
            }
        
            // 스크린 좌표를 캔버스 좌표로 변환
            indicator[i].position = screenPos;
        }
    }

    public void BindGate(Gate gate)
    {
        _gates.Add(gate);
    }

    public void UnBindGate(Gate gate)
    {
        if (!_gates.Contains(gate)) return;
        
        _gates.Remove(gate);
        indicator[(int)gate.roomDirection].gameObject.SetActive(false);
    }
}
