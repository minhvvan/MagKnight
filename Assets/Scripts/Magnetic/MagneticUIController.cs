using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public struct MagnetUIColorGroup
{
    public Color32[] colors;
}

public class MagneticUIController : MonoBehaviour
{
    [Header("Target Prefab")]
    public MagneticTarget magneticTargetPrefab; //타겟 프리팹
    
    [Header("TypeVisual Aim")]
    public Image magneticTypeVisualAim;
    
    [Header("Traking Aim")]
    public RectTransform trakingCircleRect;
    public Image trakingCircleImage;
    public Vector2 trakingCircleOrigin;
    public float trakerRotateSpeed;
    
    private CanvasGroup _trakingCircleCanvasGroup;
    private Image[] _trackingGroupImages;
    private bool _isRotate = false;
    
    [Header("SideTri Aim")]
    public RectTransform triAimLeftRectTr;
    public RectTransform triAimRightRectTr;
    public float triAimOffset;
    public float triAimOrigin;
    
    [Header("FocusAreaCircle")]
    public GameObject focusAreaCircle;
    private CanvasGroup _focusCircleCanvasGroup;
    
    [Header("CoolDown")]
    public GameObject coolDownPanel;
    private CanvasGroup _coolDownCanvasGroup;
    public Image coolDownIcon;
    private CanvasGroup _coolDownIconCanvasGroup;
    public Image coolDownFill;
    public float iconAlphaLow;
    public float fillLowValue;
    
    [Header("MagneticTypeVisual")]
    public List<MagnetUIColorGroup> circleUIColors;
    private Image[] _magneticTypeImages;
    
    [Header("Ready Container")]
    public Transform imageContainer;
    public int poolSize = 20;
    
    private Queue<MagneticTarget> _targetImgPool = new Queue<MagneticTarget>(); // 타겟 이미지 풀링
    private List<MagneticTarget> _currentTargetList = new List<MagneticTarget>(); //화면 내에 표기되는 타겟 리스트

    private WaitForSecondsRealtime _waitDoTween;
    private Coroutine _hideCoolDownUICoroutine;
    
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
        magneticTypeVisualAim.gameObject.SetActive(false);
        
        _focusCircleCanvasGroup = focusAreaCircle.GetComponent<CanvasGroup>();
        _coolDownCanvasGroup = coolDownPanel.GetComponent<CanvasGroup>();
        _coolDownIconCanvasGroup = coolDownIcon.GetComponent<CanvasGroup>();
        _trakingCircleCanvasGroup = trakingCircleImage.GetComponent<CanvasGroup>();
        
        _magneticTypeImages = focusAreaCircle.GetComponentsInChildren<Image>();
        _trackingGroupImages = trakingCircleRect.GetComponentsInChildren<Image>();
        
        _focusCircleCanvasGroup.alpha = 0;
        _trakingCircleCanvasGroup.alpha = 0;
        _coolDownCanvasGroup.alpha = 0;
        
        focusAreaCircle.transform.DOScale(Vector3.zero, 0);
        trakingCircleRect.transform.DOScale(Vector3.zero, 0);
        trakingCircleOrigin = trakingCircleRect.anchoredPosition;

        _dotDuration = 0.2f;
        _waitDoTween = new WaitForSecondsRealtime(_dotDuration);
    }
    
    public IEnumerator ShowFocusArea()
    {
        StartCoroutine(UIScale(focusAreaCircle.transform, Vector3.one, _dotDuration));
        StartCoroutine(UIFade(_focusCircleCanvasGroup, 1f, _dotDuration));
        StartCoroutine(UIScale(trakingCircleRect.transform, Vector3.one, _dotDuration));
        StartCoroutine(UIFade(_trakingCircleCanvasGroup, 1f, _dotDuration));
        //StartCoroutine(UIScale(coolDownPanel.transform, Vector3.one, _dotDuration));
        
        StartCoroutine(SetRotateCircle());
        
        yield return _waitDoTween;
    }

    public IEnumerator HideFocusArea()
    {
        StartCoroutine(UIScale(focusAreaCircle.transform, Vector3.zero, _dotDuration));
        StartCoroutine(UIFade(_focusCircleCanvasGroup, 0f, _dotDuration));
        StartCoroutine(UIScale(trakingCircleRect.transform, Vector3.zero, _dotDuration));
        StartCoroutine(UIFade(_trakingCircleCanvasGroup, 0f, _dotDuration));
        //StartCoroutine(UIScale(coolDownPanel.transform, Vector3.zero, _dotDuration));
        
        _isRotate = false;
        yield return _waitDoTween;
    }

    //중앙 원형에임 UI의 락온 여부에 따른 색상을 변경합니다.
    public IEnumerator SetTargetMagneticTypeColor(MagneticType? type = null)
    {
        if (type.HasValue)
        {
            var colorGroup = circleUIColors[2];
            trakingCircleImage.color = colorGroup.colors[0];
        }
        else
        {
            trakingCircleImage.color  = Color.white;
        }
        
        yield break;
    }

    //중앙 원형에임 UI를 회전시킵니다.
    public IEnumerator SetRotateCircle()
    {
        if (_isRotate) yield break;
        _isRotate = true;
        var angle = 0f;
        while (_isRotate)
        {
            angle = trakerRotateSpeed * Time.unscaledDeltaTime;
            trakingCircleImage.transform.Rotate(Vector3.forward, angle);
            yield return null;
        }
    }

    //락온 대상을 추적하는 < > 형태의 에임의 상황별 위치를 조정합니다.
    public IEnumerator SetTriAimPosition(bool isOn)
    {
        if (isOn) //락온 시
        {
            triAimLeftRectTr.anchoredPosition = new Vector2(-triAimOffset, 0);
            triAimRightRectTr.anchoredPosition = new Vector2(triAimOffset, 0);
        }
        else //평시 상태로 돌아올 시
        {
            triAimLeftRectTr.anchoredPosition = new Vector2(-triAimOrigin, 0);
            triAimRightRectTr.anchoredPosition = new Vector2(triAimOrigin, 0);
        }
        
        yield break;
    }

    //전체 MagneticAim컬러를 일괄 설정합니다.
    public IEnumerator SetMagneticTypeVisual(MagneticType type)
    {
        var colorGroup = circleUIColors[(int)type];
        magneticTypeVisualAim.color = colorGroup.colors[0];
        magneticTypeVisualAim.gameObject.SetActive(true);
        if (_magneticTypeImages == null) yield break;
        for (int i = 0; i < _magneticTypeImages.Length; i++)
        {
            //if (i != 0) trackingGroupImages[i].color = colorGroup.colors[i];
            _magneticTypeImages[i].color = i == 0 ? colorGroup.colors[1] : colorGroup.colors[0];
        }
        yield break;
    }
    
    //극성 전환 후, 다음 재사용까지의 대기시간을 쿨타임UI로 알려줍니다.
    public IEnumerator MagneticSwitchCoolDownVisual(float duration)
    {
        if (_hideCoolDownUICoroutine != null)
        {
            StopCoroutine(_hideCoolDownUICoroutine);
            _hideCoolDownUICoroutine = null;
        }
        
        _coolDownIconCanvasGroup.alpha = iconAlphaLow;
        StartCoroutine(UIFade(_coolDownCanvasGroup, 1f, _dotDuration));
        var elapsedTime = 0f;
        coolDownFill.fillAmount = fillLowValue;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            coolDownFill.fillAmount = Mathf.Lerp(fillLowValue, 1f, (elapsedTime / duration));
            yield return null;
        }
        _hideCoolDownUICoroutine = StartCoroutine(UIFade(_coolDownIconCanvasGroup, 1f, _dotDuration, () =>
        {
            StartCoroutine(UIFade(_coolDownCanvasGroup, 0f, _dotDuration));
        }));
    }

    //극성전환 시 MagneticAim을 좌우로 회전시킵니다.
    public IEnumerator SwitchMagneticTypeVisual(MagneticType type)
    {
        StartCoroutine(UIRotate(focusAreaCircle.transform, -45f, 0.2f, () =>
        {
            StartCoroutine(SetMagneticTypeVisual(type));
            StartCoroutine(UIRotate(focusAreaCircle.transform, 45f, 0.2f));
        }));
        yield break;
    }

    //DOTween은 TimeScale이 느려지면 같이 느려진다. 코루틴은 그것에 영향을 받지 않는다.
    
    //TimeScale에 영향을 받지 않는 DOTween DOScale!
    public IEnumerator UIScale(Transform target ,Vector3 scale, float duration, Action onComplete = null)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            var EaseFunction = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
            var t = EaseFunction(0, 1, elapsedTime / duration);
            target.localScale = Vector3.Lerp(target.localScale, scale, t);
            yield return null;
        }
        onComplete?.Invoke();
    }

    //TimeScale에 영향을 받지 않는 DOTween DOFade!
    public IEnumerator UIFade(CanvasGroup target, float value, float duration,  Action onComplete = null)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            target.alpha = Mathf.Lerp(target.alpha, value, elapsedTime / duration);
            yield return null;
        }
        onComplete?.Invoke();
    }
    
    //TimeScale에 영향을 받지 않는 DOTween DORotate!
    public IEnumerator UIRotate(Transform target, float angle, float duration,  Action onComplete = null)
    {
        var elapsedTime = 0f;
        var targetAngle = new Vector3(target.localEulerAngles.x, target.localEulerAngles.y, target.localEulerAngles.z);
        var finishAngle = new Vector3(targetAngle.x, targetAngle.y, targetAngle.z + angle);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            targetAngle = new Vector3(target.localEulerAngles.x, target.localEulerAngles.y, target.localEulerAngles.z);
            target.localRotation = Quaternion.Lerp(Quaternion.Euler(targetAngle), Quaternion.Euler(finishAngle), elapsedTime / duration);
            
            yield return null;
        }
        onComplete?.Invoke();
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
        
        foreach (var targetObj in _currentTargetList.Where(targetObj => targetObj.target == target).ToList())
        {
            targetObj.LostTarget();
            targetObj.UnlockTarget();
            _currentTargetList.Remove(targetObj);
        }
    }

    //플레이어의 정면 탐색범위 내에 들어온 대상을 조준 중임을 알립니다.
    public void InLockOnTarget(Transform target)
    {
        if (_isDispose) return;
       
        foreach (var targetObj in _currentTargetList) 
        {
            if (targetObj.target == target)
            {
                StartCoroutine(SetTriAimPosition(true));
                targetObj.onTrakingCirclePos = SetTrakingCircle;
                targetObj.LockTarget();
            }
            else
            {
                targetObj.onTrakingCirclePos = null;
                targetObj.UnlockTarget();
            }
        }
    }

    // 플레이어의 정면 탐색범위 밖으로 벗어난 대상을 다시 Ready상태로 되돌립니다.
    public void UnLockOnTarget()
    {
        if (_isDispose) return;
        
        foreach (var targetObj in _currentTargetList)
        {
            targetObj.onTrakingCirclePos = null;
            targetObj.UnlockTarget();
        }
        StartCoroutine(SetTargetMagneticTypeColor());
        StartCoroutine(SetTriAimPosition(false));
        trakingCircleRect.anchoredPosition = trakingCircleOrigin;
    }
    
    //모든 대상을 타겟대상에서 제외합니다.
    public void AllUnCountTarget()
    {
        foreach (var targetObj in _currentTargetList)
        {
            targetObj.LostTarget();
        }
    }

    public void SetTrakingCircle(Vector2 pos)
    {
        trakingCircleRect.anchoredPosition = pos;
    }
    
    #endregion

    private void OnDestroy()
    {
        //DOTween.KillAll();

        _currentTargetList.Clear();
        DisposePooling();
    }
}
