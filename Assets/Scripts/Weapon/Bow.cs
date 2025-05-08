using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Bow : BaseWeapon
{
    [SerializeField] GameObject _hitEffectPrefab;
    [SerializeField] int skillIndex;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _arrowPrefab;
    [SerializeField] GameObject muzzleVFXPrefab;
    [SerializeField] GameObject vfxMuzzle;
    [SerializeField] GameObject projectilePrefab;

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

    public override Projectile CreateProjectile(int projectileLaunchMode = 0)
    {
        ProjectileLaunchData projectileLaunchData;
        
        if (projectileLaunchMode == 1)
        {
            var launchDirection = transform.forward - transform.up;
            
            projectileLaunchData = new ProjectileLaunchData(launchDirection);
        }
        else
        {
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _layerMask))
            {
                projectileLaunchData = new ProjectileLaunchData(hit.point, true);
            }
            else
            {
                projectileLaunchData = new ProjectileLaunchData(ray.direction);
            }
        }
        
        Quaternion rotation = Quaternion.LookRotation(transform.forward, transform.up);
        Projectile projectile = ProjectileFactory.Create(projectilePrefab, transform.position, Quaternion.identity, projectileLaunchData);
        
        //SFX 재생
        var bowSfxRandomClip = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Player.Attack.Bow);
        var arrowSfxRandomClip = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Player.Attack.Arrow);
        AudioManager.Instance.PlaySFX(bowSfxRandomClip);
        AudioManager.Instance.PlaySFX(arrowSfxRandomClip);
        
        //VFX 재생
        if (muzzleVFXPrefab)
        {
            VFXManager.Instance.TriggerVFX(muzzleVFXPrefab, vfxMuzzle.transform.position);
        }
        
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
        
        //SFX
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Attack.Hit[0]);
       
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
