using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using hvvan;
using UnityEngine;

public class EnemyMagnetActionController : MonoBehaviour
{
    private Enemy _enemy;
    Animator _animator;

    public Enemy Enemy => _enemy;

    // [SerializeField] ElectricLine _electricLine;
    // public ElectricLine ElectricLine => _electricLine;

    void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        // _electricLine.gameObject.SetActive(false);
    }

    public Vector3 GetCenterPosition(Transform target)
    {
        var collider = target.GetComponent<Collider>();
        var centerPos = collider.bounds.center;
        return centerPos; 
    }


    public void StartMagnetDash(MagneticObject caster)
    {
        // enemy position
        var enemyPos = transform.position;
        var enemyCenterPos = GetCenterPosition(transform);
    
        // 타겟 포지션
        var casterPos = caster.transform.position;
        var casterCollider = caster.GetComponent<Collider>();
        var casterCenterPos = casterCollider.bounds.center;

        // 타겟 크기
        var casterWidth = casterCollider.bounds.size.x;

        // 방향벡터
        var targetVector = enemyPos - casterCenterPos;
        var targetVectorRemoveY = new Vector3(targetVector.x, 0f, targetVector.z);
        
        Vector3 casterFrontPos = casterPos + targetVectorRemoveY.normalized * casterWidth * 1.5f;

        // 물리 연산에 필요한 요소
        float distance = Vector3.Distance(enemyPos, casterFrontPos);
        float speed = 30f;
        float dashDuration =  distance / speed;
        float hitTiming = Mathf.Clamp(dashDuration - 0.15f, 0, dashDuration);
        bool isCloseTarget = distance < 3f;

        Sequence sequence = DOTween.Sequence();

        //회전
        sequence.Join(transform.DOLookAt(casterPos, 0.05f, AxisConstraint.Y)
                .SetEase(Ease.OutCubic));

        // 대쉬 시작
        sequence.AppendCallback(()=>{                
            //너무 가까운 경우 돌진 효과 없음
            if(!isCloseTarget) 
            {
                Quaternion rotation = Quaternion.LookRotation(casterPos - enemyPos);
                
                VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_RED, transform.position, rotation);
                VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_BLUE, transform.position, rotation);
            }

            StartCoroutine(MagnetDashCoroutine(caster.transform, dashDuration));
        });
    }

    public void StartMagneticPull(MagneticObject caster)
    {
        Vector3 enemyNearPoint = Vector3.Lerp(_enemy.transform.position, caster.transform.position, 0.1f);
        enemyNearPoint.y = _enemy.transform.position.y;
        StartCoroutine(MagneticPullCoroutine(caster, enemyNearPoint, 0.1f));
    }
    
#region Util functions
    Vector3 GetTargetFrontPosition(Transform target, Vector3 startPos){
        var targetCollider = target.GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
        var targetWidth = targetCollider.bounds.size.x;
        var toTargetVector = startPos - targetCenterPos;
        var toTargetVectorRemoveY = new Vector3(toTargetVector.x, 0f, toTargetVector.z);
        Vector3 targetFrontPos = targetCenterPos + toTargetVectorRemoveY.normalized * targetWidth * 3f - (targetCollider.bounds.size.y * 0.55f * Vector3.up);
        // targetFrontPos.y = 0f;

        return targetFrontPos;
    }
#endregion 

    IEnumerator MagnetDashCoroutine(Transform destinationTranform, float duration, EasingFunction.Ease easeType = EasingFunction.Ease.Linear)
    {
        _enemy.isDashing = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        Vector3 desFrontPos = GetTargetFrontPosition(destinationTranform, startPos);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            var EaseFunction = EasingFunction.GetEasingFunction(easeType);
            
            Vector3 nextPos = Vector3.Lerp(startPos, desFrontPos, EaseFunction(0, 1, t));
            Vector3 delta = nextPos - transform.position;
            _enemy.transform.position += delta;
            _enemy.Agent.nextPosition += delta;

            yield return null;
        }
        _enemy.Agent.SetDestination(destinationTranform.position);
        
        _enemy.isDashing = false;
    }

    IEnumerator MagneticPullCoroutine(MagneticObject caster, Vector3 destination, float duration,
        EasingFunction.Ease easeType = EasingFunction.Ease.Linear)
    {
        GameManager.Instance.Player.inMagnetSkill = true;
        float elapsed = 0f;
        Vector3 startPos = caster.transform.position;
        Vector3 prevPos = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            var EaseFunction = EasingFunction.GetEasingFunction(easeType);
            
            Vector3 nextPos = Vector3.Lerp(startPos, destination, EaseFunction(0, 1, t));
            Vector3 delta = nextPos - prevPos;
            caster.transform.position += delta;
            prevPos = nextPos;
            
            yield return null;
        }
        GameManager.Instance.Player.inMagnetSkill = false;
        _enemy.Agent.SetDestination(destination);
    }
}
