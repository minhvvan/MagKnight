
using System;
using UnityEngine;

public class Katana: BaseWeapon
{
    public override void AttackStart()
    {
        base.AttackStart();
        
        //TODO: FX
    }

    public override void AttackEnd()
    {
        base.AttackEnd();

    }

    public override void OnNext(HitInfo hitInfo)
    {
        Debug.Log($"{hitInfo.hit.collider.name} : Hit at {hitInfo.time:F1}s");
    }

    public override void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public override void OnCompleted()
    {
    }
    
    public override void ChangePolarity()
    {
        //TODO: 극성 스위칭 효과
        // 2초간 대쉬 쿨타임 없음
    }
}
