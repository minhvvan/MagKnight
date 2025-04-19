using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MagneticTarget : MonoBehaviour
{
    public Sprite[] targetImg;
    public Color32[] targetColor;
    
    public RectTransform rectTransform;
    private readonly Vector2 _readySize = new Vector2(118,119);
    private readonly Vector2 _lockSize = new Vector2(279,279);
    
    public Transform target;
    public Action<MagneticTarget> onReturnTarget;
    
    private Image _currentImg;
    private Camera _mainCamera;
    private Canvas _uiCanvas;
    private RectTransform _canvasRectTransform;
    
    private bool _haveTarget = false;
    
    private bool _isLocked = false;
    public bool IsTargetLock
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            _currentImg.sprite = !_isLocked ? targetImg[0] : targetImg[1];
            _currentImg.color = !_isLocked ? targetColor[0] : targetColor[1];
            rectTransform.sizeDelta = !_isLocked ? _readySize : _lockSize;
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (target != null)
        {
            rectTransform.anchoredPosition = OperateUiPoint(_mainCamera.WorldToScreenPoint(target.position));
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
        rectTransform = GetComponent<RectTransform>();
        _currentImg = GetComponent<Image>();
        _uiCanvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _uiCanvas.GetComponent<RectTransform>();

        IsTargetLock = false;
    }

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
