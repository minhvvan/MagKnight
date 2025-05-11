using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MagneticTarget : MonoBehaviour
{
    [Header("Image & Color Set")]
    public Sprite[] targetImg;
    public Color32[] targetColor;
    
    [NonSerialized] public Transform target;
    public Action<MagneticTarget> onReturnTarget;
    public Action<Vector2> onTrakingCirclePos;//MagneticUI의 원형 Aim 추적용
    
    [Header("Image width & height Set")]
    public Vector2 readySize = new Vector2(118,119);
    public Vector2 lockSize = new Vector2(279,279);
    
    private Image _currentImg;
    private Camera _mainCamera;
    private Canvas _uiCanvas;
    private RectTransform _rectTransform;
    private RectTransform _canvasRectTransform;
    
    private bool _haveTarget = false;
    
    private bool _isLocked = false;
    public bool IsTargetLock
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            if(_currentImg == null) Initialize();
            _currentImg.sprite = !_isLocked ? targetImg[0] : targetImg[1];
            _currentImg.color = !_isLocked ? targetColor[0] : targetColor[1];
            _rectTransform.sizeDelta = !_isLocked ? readySize : lockSize;
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (target != null)
        {
            var targetPosition = target.position;
            
            // if(target.TryGetComponent(out Enemy enemy))
            // {
            //     targetPosition = enemy.blackboard.headTransform.position;
            // }
            
            if(_mainCamera == null) Initialize();
            _rectTransform.anchoredPosition = OperateUiPoint(_mainCamera.WorldToScreenPoint(targetPosition));
            onTrakingCirclePos?.Invoke(OperateUiPoint(_mainCamera.WorldToScreenPoint(targetPosition)));
        }
        else
        {
            if (!_haveTarget) return;
            onReturnTarget?.Invoke(this);
            _haveTarget = false;
        }
    }

    private void Initialize()
    {
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
        _currentImg = GetComponent<Image>();
        _uiCanvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _uiCanvas.GetComponent<RectTransform>();

        IsTargetLock = false;
    }

    //
    private Vector2 OperateUiPoint(Vector3 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform, screenPoint, _uiCanvas.worldCamera, out var uiPosition);
        
        return uiPosition;
    }
    
    public void SetTarget(Transform target)
    {
        this.target = target;
        _haveTarget = true;
        gameObject.SetActive(true);
    }

    public void LostTarget()
    {
        target = null;
    }

    public void LockTarget()
    {
        IsTargetLock = true;
    }

    public void UnlockTarget()
    {
        IsTargetLock = false;
    }
}
