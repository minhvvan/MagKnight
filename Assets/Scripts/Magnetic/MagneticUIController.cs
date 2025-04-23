using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MagneticUIController : MonoBehaviour
{
    [Header("Target Prefab")]
    public MagneticTarget magneticTargetPrefab; //타겟 프리팹
    
    [Header("Ready Container")]
    public Transform imageContainer;
    public int poolSize = 20;
    
    [Header("FocusAreaCircle")]
    public GameObject focusAreaCircle;
    private CanvasGroup _focusCircleCanvasGroup;
    
    [Header("MagneticTypeVisual")]
    public GameObject magneticTypeN;
    public GameObject magneticTypeS;
    private CanvasGroup _typeNCanvasGroup;
    private CanvasGroup _typeSCanvasGroup;
    
    private Queue<MagneticTarget> _targetImgPool = new Queue<MagneticTarget>(); // 타겟 이미지 풀링
    private List<MagneticTarget> _currentTargetList = new List<MagneticTarget>(); //화면 내에 표기되는 타겟 리스트

    private WaitForSeconds _waitDoTween;
    
    private bool _isDispose = false; //true 시 모든 동작 중단.
    private float _dotDuration;
    
    private void Awake()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        InitializePooling();
        InitializeUI();
    }

    #region Visualize UI

    private void InitializeUI()
    {
        _focusCircleCanvasGroup = focusAreaCircle.GetComponent<CanvasGroup>();
        _typeNCanvasGroup = magneticTypeN.GetComponent<CanvasGroup>();
        _typeSCanvasGroup = magneticTypeS.GetComponent<CanvasGroup>();
        
        _focusCircleCanvasGroup.alpha = 0;
        _typeNCanvasGroup.alpha = 0;
        _typeSCanvasGroup.alpha = 0;
        
        focusAreaCircle.transform.DOScale(Vector3.zero, 0);
        magneticTypeN.transform.DOScale(Vector3.zero, 0);
        magneticTypeS.transform.DOScale(Vector3.zero, 0);

        _dotDuration = 0.2f;
        _waitDoTween = new WaitForSeconds(_dotDuration);
    }
    
    public IEnumerator ShowFocusArea()
    {
        // focusAreaCircle.transform.DOScale(Vector3.one, _dotDuration);
        // _focusCircleCanvasGroup.DOFade(1, _dotDuration);
        StartCoroutine(UIScale(focusAreaCircle.transform, Vector3.one, _dotDuration));
        StartCoroutine(UIFade(_focusCircleCanvasGroup, 1f, _dotDuration));
        
        yield return _waitDoTween;
    }

    public IEnumerator HideFocusArea()
    {
        // focusAreaCircle.transform.DOScale(Vector3.zero, _dotDuration);
        // _focusCircleCanvasGroup.DOFade(0, _dotDuration);
        
        StartCoroutine(UIScale(focusAreaCircle.transform, Vector3.zero, _dotDuration));
        StartCoroutine(UIFade(_focusCircleCanvasGroup, 0f, _dotDuration));
        yield return _waitDoTween;
    }

    public IEnumerator ShowMagneticTypeVisual(MagneticType type)
    {
        switch (type)
        {
            case MagneticType.N:
                if (_typeNCanvasGroup.alpha == 0 && _typeSCanvasGroup.alpha == 0)
                {
                    // magneticTypeN.transform.DOScale(Vector3.one, _dotDuration);
                    // magneticTypeS.transform.DOScale(Vector3.one, _dotDuration);
                    // _typeNCanvasGroup.DOFade(1, _dotDuration);
                    StartCoroutine(UIScale(magneticTypeN.transform, Vector3.one, _dotDuration));
                    StartCoroutine(UIScale(magneticTypeS.transform, Vector3.one, _dotDuration));
                    StartCoroutine(UIFade(_typeNCanvasGroup, 1f, _dotDuration));
                    
                    yield return _waitDoTween;
                    
                    yield break;
                }
                _typeNCanvasGroup.DOFade(1, 0);
                _typeSCanvasGroup.DOFade(0, 0);
                break;
            case MagneticType.S:
                if (_typeNCanvasGroup.alpha == 0 && _typeSCanvasGroup.alpha == 0)
                {
                    // magneticTypeN.transform.DOScale(Vector3.one, _dotDuration);
                    // magneticTypeS.transform.DOScale(Vector3.one, _dotDuration);
                    // _typeSCanvasGroup.DOFade(1, _dotDuration);
                    StartCoroutine(UIScale(magneticTypeN.transform, Vector3.one, _dotDuration));
                    StartCoroutine(UIScale(magneticTypeS.transform, Vector3.one, _dotDuration));
                    StartCoroutine(UIFade(_typeSCanvasGroup, 1f, _dotDuration));
                    
                    yield return _waitDoTween;
                    
                    yield break;
                }
                _typeNCanvasGroup.DOFade(0, 0);
                _typeSCanvasGroup.DOFade(1, 0);
                break;
        }
    }

    public IEnumerator HideMagneticTypeVisual()
    {
        // magneticTypeN.transform.DOScale(Vector3.zero, _dotDuration);
        // magneticTypeS.transform.DOScale(Vector3.zero, _dotDuration);
        //
        // _typeNCanvasGroup.DOFade(0, _dotDuration);
        // _typeSCanvasGroup.DOFade(0, _dotDuration);
        
        StartCoroutine(UIScale(magneticTypeN.transform, Vector3.zero, _dotDuration));
        StartCoroutine(UIScale(magneticTypeS.transform, Vector3.zero, _dotDuration));
        StartCoroutine(UIFade(_typeNCanvasGroup, 0, _dotDuration));
        StartCoroutine(UIFade(_typeSCanvasGroup, 0, _dotDuration));

        yield return _waitDoTween;
    }

    //DOTween은 TimeScale이 느려지면 같이 느려진다. 코루틴은 그것에 영향을 받지 않는다.
    
    //TimeScale에 영향을 받지 않는 DOTween DOScale!
    public IEnumerator UIScale(Transform target ,Vector3 scale, float duration)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            target.localScale = Vector3.Lerp(target.localScale, scale, elapsedTime / duration);
            yield return null;
        }
    }

    //TimeScale에 영향을 받지 않는 DOTween DOFade!
    public IEnumerator UIFade(CanvasGroup target, float value, float duration)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            target.alpha = Mathf.Lerp(target.alpha, value, elapsedTime / duration);
            yield return null;
        }
    }

    #endregion

    #region 타겟 풀링
    
    private void InitializePooling()
    {
        //targetImage Pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateTargetImg();
        }
    }

    private void DisposePooling()
    {
        while (_targetImgPool.Count > 0)
        {
            Destroy(_targetImgPool.Dequeue());
        }
    }

    private void CreateTargetImg()
    {
        var targetObj = Instantiate(magneticTargetPrefab, imageContainer);
        targetObj.gameObject.SetActive(false);
        _targetImgPool.Enqueue(targetObj);
    }

    private MagneticTarget GetTargetImg()
    {
        if (_targetImgPool.Count <= 0) CreateTargetImg();

        return _targetImgPool.Dequeue();
    }

    public void ReturnTargetImg(MagneticTarget targetObj)
    {
        targetObj.onReturnTarget = null;
        targetObj.gameObject.SetActive(false);
        
        _targetImgPool.Enqueue(targetObj);
    }
    
    #endregion
    
    #region 타겟팅 관리
    
    //범위 내 감지된 대상을 TargetLock 대기 상태로 올립니다.
    public void InCountTarget(Transform target)
    {
        if (_isDispose) return;
        
        //이미 존재하는 대상이면 return
        if (_currentTargetList.Any(targetObj => targetObj.target == target))
        {
            return;
        }
        
        var targetObj = GetTargetImg();
        targetObj.SetTarget(target);
        targetObj.onReturnTarget = ReturnTargetImg;
        
        _currentTargetList.Add(targetObj);
    }

    //범위 밖으로 벗어난 대상을 확인하고 추적중인 TargetLock을 비활성화 시킵니다.
    public void UnCountTarget(Transform target)
    {
        if (_isDispose) return;
        
        //일치하는 대상이 없으면 return
        if (_currentTargetList.All(targetObj => targetObj.target != target))
        {
            return;
        }
        
        foreach (var targetObj in _currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.LostTarget();
        }
    }

    //플레이어의 정면 탐색범위 내에 들어온 대상을 조준 중임을 알립니다.
    public void InLockOnTarget(Transform target)
    {
        if (_isDispose) return;
        
        foreach (var targetObj in _currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.LockTarget();
        }
    }

    // 플레이어의 정면 탐색범위 밖으로 벗어난 대상을 다시 Ready상태로 되돌립니다.
    public void UnLockOnTarget(Transform target)
    {
        if (_isDispose) return;
        
        foreach (var targetObj in _currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.UnlockTarget();
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        DOTween.KillAll();
        _currentTargetList.Clear();
        DisposePooling();
    }
}
