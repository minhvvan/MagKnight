
using System;
using UnityEngine;

public class Katana: BaseWeapon
{
    [SerializeField] GameObject _hitEffectPrefab;

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
        base.OnNext(hitInfo);
        //FX
        GameObject hitEffect = Instantiate(_hitEffectPrefab, hitInfo.hit.point, Quaternion.identity);
        hitEffect.transform.forward = hitInfo.hit.normal;
        hitEffect.transform.localScale = Vector3.one * 0.3f;
        Destroy(hitEffect, 0.2f);

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
