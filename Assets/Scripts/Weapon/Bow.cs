using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : BaseWeapon
{
    [SerializeField] GameObject _hitEffectPrefab;
    [SerializeField] int skillIndex;

    [SerializeField] GameObject _arrowPrefab;

    private Camera _mainCamera;
    private float _rayDistance = 100f;
    
    WeaponTrail _weaponTrail;

    void Start()
    {
        _mainCamera = Camera.main;
    }
    

    public override void AttackStart(int hitboxGroupId)
    {

        
        
    }

    public override Projectile CreateProjectile(GameObject projectilePrefab)
    {
        // ProjectileFactory.Create(_arrowPrefab, transform.position, Quaternion.identity, )
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        ProjectileLaunchData projectileLaunchData;
        if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance))
        {
            projectileLaunchData = new ProjectileLaunchData(hit.point, true);
            
        }
        else
        {
            projectileLaunchData = new ProjectileLaunchData(ray.direction);
        }
        Projectile projectile = ProjectileFactory.Create(projectilePrefab, transform.position, Quaternion.identity, projectileLaunchData);
        return projectile;
    }

    public override void AttackEnd(int hitboxGroupId)
    {
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
