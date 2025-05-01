using System;
using System.Collections;
using DG.Tweening;
using Moon;
using UnityEngine;

public class PlayerMagnetActionController : MonoBehaviour
{
    PlayerController _playerController;
    Animator _animator;
    

    #region Magnet Actions
    [SerializeField] ElectricLine _electricLine;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        _electricLine.gameObject.SetActive(false);
    }

    public void StartMagnetDash(MagneticObject caster)
    {
        _animator.Play(PlayerAnimatorConst.hashMagnetSkillDash, 0, 0.2f);

        var targetPos = transform.position;
        var targetCollider = GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
        
        var casterPos = caster.transform.position;
        var casterCollider = caster.GetComponent<Collider>();
        var casterCenterPos = casterCollider.bounds.center;

        var casterWidth = casterCollider.bounds.size.x;

        var targetVector = targetPos - casterCenterPos;
        var targetVectorRemoveY = new Vector3(targetVector.x, 0f, targetVector.z);

        Vector3 casterFrontPos = casterPos + targetVectorRemoveY.normalized * casterWidth * 1.5f;

        //제어를 위함 플레이어 공중에 살짝 붕 뜨는 모션
        _playerController.inMagnetSkill = true;

        float distance = Vector3.Distance(targetPos, casterFrontPos);
        float speed = 30f;
        float dashDuration =  distance / speed;
        float hitTiming = Mathf.Clamp(dashDuration - 0.15f, 0, 1);
        bool isCloseTarget = distance < 3f;

        Sequence sequence = DOTween.Sequence();
        
        // Step 1: 살짝 뜨기
        sequence.Append(transform.DOMove(targetPos, 0.05f)
            .SetEase(Ease.OutCubic)
            .OnStart(() => {
                VFXManager.Instance.TriggerVFX(VFXType.MAGNET_ACTION_EXPLOSION, targetCenterPos, Quaternion.identity);
                _electricLine.startPosition = targetCenterPos;
                _electricLine.endPosition = casterCenterPos;
                _electricLine.gameObject.SetActive(true);

                Time.timeScale = 0.2f;
                if(!isCloseTarget){
                    StartCoroutine(_playerController.cameraSettings.AdjustFOV(50f, 80f, 0.2f));
                    MotionBlurController.Play(0.8f, 0.1f);
                } 
            })
            .OnComplete(() => {
                Time.timeScale = 1f;
                _electricLine.gameObject.SetActive(false);
            })); 

        //회전
        sequence.Join(transform.DOLookAt(casterPos, 0.05f, AxisConstraint.Y)
                .SetEase(Ease.OutCubic));

        //딜레이
        sequence.AppendInterval(0.1f);

        // Step 2: 대쉬 시작
        sequence.AppendCallback(()=>{                
            if(!isCloseTarget) StartCoroutine(_playerController.cameraSettings.AdjustFOV(80f, 50f, 0.2f));
            
            VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_RED, transform.position, transform.rotation);
            VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_BLUE, transform.position, transform.rotation);

            StartCoroutine(MagnetDashCoroutine(caster.transform, dashDuration, hitTiming, () => {
                    MotionBlurController.Play(0, 0.1f);
                    Time.timeScale = 1f;
                    _playerController.inMagnetSkill = false;
                }));
        });
    }

    Vector3 GetTargetFrontPosition(Transform target, Vector3 startPos){
        var targetCollider = target.GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
        var targetWidth = targetCollider.bounds.size.x;
        var toTargetVector = startPos - targetCenterPos;
        var toTargetVectorRemoveY = new Vector3(toTargetVector.x, 0f, toTargetVector.z);
        Vector3 targetFrontPos = targetCenterPos + toTargetVectorRemoveY.normalized * targetWidth * 1.5f - (targetCollider.bounds.size.y * 0.5f * Vector3.up);

        return targetFrontPos;
    }

    IEnumerator MagnetDashCoroutine(Transform destinationTranform, float duration, float hitTiming, Action onComplete)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        bool hasAttacked = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            Vector3 desFrontPos = GetTargetFrontPosition(destinationTranform, startPos);


            Vector3 nextPos = Vector3.Lerp(startPos, desFrontPos, t);
            Vector3 delta = nextPos - transform.position;
            _playerController.characterController.Move(delta);
            

            if (!hasAttacked && elapsed >= hitTiming)
            {
                hasAttacked = true;
                _playerController.StartNormalAttack();
            }

            yield return null;
        }

        // 종료 처리
        onComplete?.Invoke();
    }
    
#endregion
}