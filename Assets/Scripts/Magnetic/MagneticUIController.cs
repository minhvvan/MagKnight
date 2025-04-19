using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MagneticUIController : MonoBehaviour
{
    public Queue<MagneticTarget> targetImgPool = new Queue<MagneticTarget>();
    public List<MagneticTarget> currentTargetList = new List<MagneticTarget>();
    
    public MagneticTarget magneticTargetPrefab;
    public Transform imageContainer;
    public int poolSize = 20;
    
    private void Awake()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        TargetPooling();
    }
    
    #region 타겟팅 관리

    //범위 내 감지된 대상을 TargetLock 대기 상태로 올립니다.
    public void InCountTarget(Transform target)
    {
        //이미 존재하는 대상이면 return
        if (currentTargetList.Any(targetObj => targetObj.target == target))
        {
            return;
        }
        
        var targetObj = GetTargetImg();
        targetObj.SetTarget(target);
        targetObj.onReturnTarget = ReturnTargetImg;
        
        currentTargetList.Add(targetObj);
    }

    //범위 밖으로 벗어난 대상을 확인하고 추적중인 TargetLock을 비활성화 시킵니다.
    public void UnCountTarget(Transform target)
    {
        //일치하는 대상이 없으면 return
        if (currentTargetList.All(targetObj => targetObj.target != target))
        {
            return;
        }
        
        foreach (var targetObj in currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.LostTarget();
        }
    }

    //플레이어의 정면 탐색범위 내에 들어온 대상을 조준 중임을 알립니다.
    public void InLockOnTarget(Transform target)
    {
        foreach (var targetObj in currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.LockTarget();
        }
    }

    // 플레이어의 정면 탐색범위 밖으로 벗어난 대상을 다시 Ready상태로 되돌립니다.
    public void UnLockOnTarget(Transform target)
    {
        foreach (var targetObj in currentTargetList.Where(targetObj => targetObj.target == target))
        {
            targetObj.UnlockTarget();
        }
    }
    
    #endregion

    #region 타겟 풀링
    
    private void TargetPooling()
    {
        //targetImage Pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateTargetImg();
        }
    }

    private void CreateTargetImg()
    {
        var targetObj = Instantiate(magneticTargetPrefab, imageContainer);
        targetObj.gameObject.SetActive(false);
        targetImgPool.Enqueue(targetObj);
    }

    private MagneticTarget GetTargetImg()
    {
        if (targetImgPool.Count <= 0) CreateTargetImg();

        return targetImgPool.Dequeue();
    }

    public void ReturnTargetImg(MagneticTarget targetObj)
    {
        targetObj.onReturnTarget = null;
        targetObj.gameObject.SetActive(false);
        
        targetImgPool.Enqueue(targetObj);
    }
    
    #endregion

    
}
