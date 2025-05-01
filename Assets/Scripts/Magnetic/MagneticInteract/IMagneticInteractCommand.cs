using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using Moon;
using UnityEngine;
using DG.Tweening;

//TODO: 시전자, 대상자만 넣으면 상호간의 액션을 실행할 수 있도록 기능 제공
public abstract class IMagneticInteractCommand
{
    public LayerMask magneticLayer; //Magnetic 레이어
    public LayerMask enemyLayer; //Enemy 레이어
    public LayerMask groundLayer; //Ground 레이어
    public LayerMask environmentLayer; //Environment 레이어
    public int maxInCount; //탐지 개체 최대 수
    
    public Vector3 _currentVelocity; // 현재 가속도
    public float dragValue; //가속 후 감속값
    
    public float minDistance;//대상 오브젝트가 일정 거리 이내로 다가올 시 자기력 상호작용을 종료할 최소거리.
    public float outBoundDistance;//자기력이 작용하는 최대 거리
    public float hangAdjustValue;//Vector만 전달 시 y축 보정
    public float counterPressRange;
    
    public float structSpeed; //구조물 대상 적용 속도
    public float nonStructSpeed; //비구조물 대상 적용 속도
    public float counterPressPower;
    
    public bool useCounterPress = false;

    public IMagneticInteractCommand()
    {
        SetData().Forget();
        
        magneticLayer = LayerMask.NameToLayer("Magnetic");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        groundLayer = LayerMask.NameToLayer("Ground");
        environmentLayer = LayerMask.NameToLayer("Environment");
    }

    public async UniTask SetData()
    {
        var data = await DataManager.Instance.LoadScriptableObjectAsync<MagneticSetupSO>
            (Addresses.Data.Magnetic.MagneticSetupData);
        
        maxInCount = data.maxInCount;
        
        dragValue = data.dragValue;
        structSpeed = data.structSpeed;
        nonStructSpeed = data.nonStructSpeed;
        
        minDistance = data.minDistance;
        outBoundDistance = data.outBoundDistance;
        hangAdjustValue = data.hangAdjustValue;//_outBoundDistance/10f+0.25f; //1.2~2f
        
        counterPressRange = data.counterPressRange;
        counterPressPower = data.counterPressPower;
    }
    
    //이동 시 겹침 보정 함수.
    public void PenetrationFix(MagneticObject magneticObject)
    {
        var obj = magneticObject.gameObject;
        var col = magneticObject.GetComponent<Collider>();
        var center = col.bounds.center;
        var size  = col.bounds.size;
        
        var radius = Mathf.Min(size.x, size.z)/2f;
        var height = size.y;
        var halfHeight = Mathf.Max(height / 2f - radius, 0f);
        var up = Vector3.up;
        var point1 = center + up * halfHeight;
        var point2 = center - up * halfHeight;

        var hitColliders = new Collider[maxInCount];
        var hitCount = Physics.OverlapCapsuleNonAlloc(point1, point2, radius, hitColliders, 
            (1 << magneticLayer) | (1 << enemyLayer) |
            (1 << groundLayer) | (1 << environmentLayer));

        //감지한 대상 기반으로 보정 수행
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCol = hitColliders[i];
            
            //본인 제외
            if(hitCol == col) continue;
            
            //ComputePenetration
            if (Physics.ComputePenetration(col, obj.transform.position, obj.transform.rotation,
                    hitCol, hitCol.transform.position, hitCol.transform.rotation,
                    out Vector3 direction, out float distance))
            {
                //겹침(침투) 보정 //겹친만큼 반대방향으로 이동시킴.
                obj.transform.position += direction * distance;
            }
        }
    }
    
    public abstract UniTask Execute(MagneticObject caster, MagneticObject target);
}

public class OnApproach : IMagneticInteractCommand
{
    public override async UniTask Execute(MagneticObject caster, MagneticObject target)
    {
        caster.TryGetComponent(out CharacterController casterCharacterController);
        target.TryGetComponent(out CharacterController targetCharacterController);
        
        var targetPos = target.transform.position;
        var targetCollider = target.GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
        
        var casterPos = caster.transform.position;
        var casterCollider = caster.GetComponent<Collider>();
        var casterCenterPos = casterCollider.bounds.center;
        
        var distance = (targetPos - casterPos).magnitude;
        var staticDistance = distance;
        
        var eta = 0f;
        var magnetSpeed = 0f;
        if (caster.GetIsStructure() || target.GetIsStructure())
        {
            magnetSpeed = structSpeed;
            eta = distance / (magnetSpeed);
        }
            
        else
        {
            magnetSpeed = nonStructSpeed;
            eta = distance / magnetSpeed;
        }

        var elapsedTime = 0f;
        var elapsedDistance = 0f;
        //방향까지 가속
        while (elapsedTime < eta) // eta/2f
        {
            if (Time.timeScale == 0f) await UniTask.WaitUntil(()=> Mathf.Approximately(Time.timeScale, 1f));
            
            elapsedTime += Time.deltaTime;
            
            //Lerp로도 잘 어울려지도록 ETA 보정
            float progressTime = Mathf.Clamp01(elapsedTime / eta);

            targetPos = target.transform.position;
            targetCenterPos = targetCollider.bounds.center;
            
            casterPos = caster.transform.position;
            casterCenterPos = casterCollider.bounds.center;
            
            //characterController가 있는 대상인지 체크.
            if (targetCharacterController != null)
            {
                //target이 caster에게 이동.
                var direction = (casterCenterPos - targetPos);
                direction.y *= hangAdjustValue; //normalized로 인한 y값 감소에 대한 보정
                
                var hangDirection= direction.normalized;

                var newMovement = hangDirection * progressTime;
                 
                _currentVelocity = newMovement;
                
                targetCharacterController.Move(newMovement);
                
                
                elapsedDistance += Vector3.Distance(casterPos+newMovement, casterPos);
            }
            else //characterController가 없는 대상.
            {
                //target이 caster에게 이동
                var newPosition = Vector3.MoveTowards( targetPos, casterCenterPos, progressTime);
                target.transform.position = (newPosition);
                elapsedDistance += Vector3.Distance(targetPos, newPosition);
                //이동 로직 수행 후 대상 보정
                PenetrationFix(target);
            }
            
            // Debug.Log("ETA" + elapsedTime);
            // if(elapsedTime >= eta) Debug.Log("ETA Timeout!");
            
            distance = (targetPos - casterCenterPos).magnitude;
            if (elapsedDistance >= staticDistance) break;
            if (distance <= minDistance)
            {
                break;
            }

            await UniTask.Yield();
        }
        
        //도착 후 관성
        if (targetCharacterController != null && !targetCharacterController.isGrounded)//공중에 떠있는 상태라면..
        {
            var quickDragValue = dragValue;
            //quickDragValue *= 3f;
            
            while (_currentVelocity != Vector3.zero)
            {
                
                if (targetCharacterController.isGrounded)
                {
                    _currentVelocity = Vector3.zero;
                    targetCharacterController.Move(_currentVelocity);
                    break;
                }
                
                _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, quickDragValue * Time.deltaTime);
                targetCharacterController.Move(_currentVelocity);

                await UniTask.Yield();
            }
        }

        
    }
}

public class OnSeparation : IMagneticInteractCommand
{
    public override async UniTask Execute(MagneticObject caster, MagneticObject target)
    {
        target.TryGetComponent(out CharacterController targetCharacterController);
        caster.TryGetComponent(out CharacterController casterCharacterController);
        
        var targetPos = target.transform.position;
        var targetCollider = target.GetComponent<Collider>();
        var targetCenterPos = targetCollider.bounds.center;
         
        var casterPos = caster.transform.position;
        var casterCollider = caster.GetComponent<Collider>();
        var casterCenterPos = casterCollider.bounds.center;
        
        var distance = (targetPos - casterCenterPos).magnitude;
        var maxDistance = useCounterPress ? counterPressRange : outBoundDistance;
        var backDistance = maxDistance - Vector3.Distance(targetPos, casterCenterPos);
        
        var direction = (targetPos - casterCenterPos).normalized;
        var destination = direction * maxDistance;
        var outDistance = Vector3.Distance(destination + casterCenterPos, targetPos);

        var readyCounter = useCounterPress;
        
        var eta = 0f;
        var magnetSpeed = 0f;
        if (caster.GetIsStructure() || target.GetIsStructure())
        {
            magnetSpeed = structSpeed;
            eta = backDistance / (magnetSpeed);
        }
        else if (useCounterPress)
        {
            eta = distance / counterPressPower;
        }    
        else
        {
            magnetSpeed = nonStructSpeed;
            eta = outDistance / magnetSpeed;
        }
        
        var elapsedTime = 0f;
        var elapsedDistance = 0f;
        
        //방향까지 가속
        while (elapsedTime < eta)
        {
            if (readyCounter && !useCounterPress) break;
            
            elapsedTime += Time.deltaTime;
            //Lerp로도 잘 어울려지도록 ETA 보정
            float progressTime = Mathf.Clamp01(elapsedTime / eta);
            
            targetPos = target.transform.position;
            targetCenterPos = targetCollider.bounds.center;
            
            casterPos = caster.transform.position;
            casterCenterPos = casterCollider.bounds.center;
            
            destination = (targetPos - casterCenterPos).normalized * maxDistance;
    
            //characterController가 있는 대상인지 체크.
            if (targetCharacterController != null)
            {
                var backDirection = destination.normalized;
                backDirection.y *= hangAdjustValue; //normalized로 인한 y값 감소에 대한 보정
                
                var newMovement = (backDirection) * progressTime;
                
                _currentVelocity = newMovement;
                targetCharacterController.Move(newMovement);
                elapsedDistance += Vector3.Distance(newMovement + casterPos, casterPos);
            }
            else //characterController가 없는 대상.
            {
                var pressDirection = destination + casterCenterPos;
                var newPosition = Vector3.MoveTowards(targetPos, pressDirection,
                    progressTime);
                target.transform.position = newPosition;
                elapsedDistance += Vector3.Distance(targetPos, newPosition);
                PenetrationFix(target);
            }
            
            distance = (targetPos - casterCenterPos).magnitude;
            if (elapsedDistance >= outDistance) break;
            if (distance >= maxDistance)
            {
                break;
            }
            
            await UniTask.Yield();
        }
    }
}

public class MagnetDashAction : IMagneticInteractCommand
{
    public override UniTask Execute(MagneticObject caster, MagneticObject target)
    {
        if(target.TryGetComponent(out PlayerMagnetActionController targetPlayerMagnetActionController))
        {
            targetPlayerMagnetActionController.StartMagnetDash(caster);
        }

        return UniTask.CompletedTask;
    }
}

public static class MagneticInteractFactory
{
    public static IMagneticInteractCommand GetInteract<T>() where T : IMagneticInteractCommand, new()
    {
        return new T();
    }
}