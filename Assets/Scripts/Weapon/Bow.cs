using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : BaseWeapon
{
    [SerializeField] GameObject _hitEffectPrefab;
    [SerializeField] int skillIndex;

    WeaponTrail _weaponTrail;

    void Start()
    {
        // _weaponTrail = GetComponent<WeaponTrail>();
    }
    

    public override void AttackStart(int hitboxGroupId)
    {
        base.AttackStart(hitboxGroupId);
        //TODO: FX
        if(_weaponTrail != null)
        {
            _weaponTrail.EnableTrail(true);
        }
    }

    public override void AttackEnd(int hitboxGroupId)
    {
        base.AttackEnd(hitboxGroupId);
        
        if(_weaponTrail != null)
        {
            _weaponTrail.EnableTrail(false);
        }
    }

    public override int OnSkill()
    {
        return skillIndex;
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
