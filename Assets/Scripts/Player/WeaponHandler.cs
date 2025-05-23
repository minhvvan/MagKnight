﻿using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Moon;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<WeaponType, Transform> _weaponSockets;
    
    [SerializeField] private BaseWeapon _currentWeapon;
    private WeaponDataDictSO _weaponDataDictionary;
    private AbilitySystem _abilitySystem;
    
    public MagCore currentMagCore;
    public WeaponType CurrentWeaponType { get; private set; }
    private bool _isActiveMagneticSwitchEffect = false;
    private Animator _animator;

    private GameplayEffect _damageEffect;
    private GameplayEffect _resistanceEffect;
    private float _currentAttackLevel;
    
    private async void Awake()
    {
        _abilitySystem = GetComponent<AbilitySystem>();
        _animator = GetComponent<Animator>();
        
        _damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, 0)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
        _resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.ResistanceDamage, 10)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
        
        try
        {
            _weaponDataDictionary =
                await DataManager.Instance.LoadScriptableObjectAsync<WeaponDataDictSO>(Addresses.Data.Weapon.Dictionary);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void SetCurrentWeapon(WeaponType weaponType)
    {
        if (_weaponDataDictionary == null)
        {
            Debug.Log("WeaponData is null");
            return;
        }

        CurrentWeaponType = weaponType;
        PlayerEvent.TriggerWeaponChange(weaponType);
        if (CurrentWeaponType == WeaponType.None)
        {
            Debug.Log("WeaponType is null");
            return;
        }

        if (_currentWeapon != null) Destroy(_currentWeapon.gameObject);
        _currentWeapon = Instantiate(_weaponDataDictionary.weapons[weaponType].prefab, _weaponSockets[weaponType]).GetComponent<BaseWeapon>();
        _currentWeapon.OnHit += OnHitAction;

        //사거리 설정
        _damageEffect.extraData.weaponRange = _weaponDataDictionary.weapons[weaponType].range;

        
    }

    public void DropPrevWeapon(Transform newCorePos)
    {
        var prevWeaponCategory = currentMagCore.category;
        var prevWeaponRarity = currentMagCore.rarity;
        var dropObj =
            ItemManager.Instance.CreateItem(prevWeaponCategory, prevWeaponRarity, newCorePos.position,
                Quaternion.identity, itemName: currentMagCore.itemName);

        //TODO: 추후 MagCore강화수치 같은게 생길 시 아래를 통해 접근하여 반영
        var magCoreData = dropObj.GetComponent<MagCore>();
        magCoreData.SetMagCoreData(newMagCore: currentMagCore);

        //이전 무기의 고유효과 제거.
        currentMagCore.RemovePartsEffect(_abilitySystem);
        
        var vfxObj = Instantiate(ItemManager.Instance.weaponChangeVfxPrefab, _currentWeapon.transform);
        vfxObj.transform.localScale *= 0.5f;
        vfxObj.transform.rotation *= Quaternion.Euler(90f,0, 0);
        Destroy(vfxObj, 0.25f);
        
        Destroy(currentMagCore.gameObject);
    }
    
    //극성 전환 효과 활성화
    public void ActivateMagnetSwitchEffect(AbilitySystem abilitySystem, MagneticType type)
    {
        // var vfxObj = Instantiate(ItemManager.Instance.magnetSwitchVfxPrefab, _currentWeapon.transform);
        // //vfxObj.transform.localScale *= 2f;
        // var vfxs = vfxObj.GetComponentsInChildren<ParticleSystem>();
        // foreach (var vfx in vfxs)
        // {
        //     var main = vfx.main;
        //     if(type == MagneticType.N) main.startColor = Color.red;
        //     else if(type == MagneticType.S) main.startColor = Color.blue;
        // }
        // Destroy(vfxObj, 0.25f);
        
        VFXType vfxType = type == MagneticType.N ? VFXType.MAGNETIC_SWITCH_N : VFXType.MAGNETIC_SWITCH_S;
        VFXManager.Instance.TriggerVFX(vfxType, _currentWeapon ? _currentWeapon.transform : transform);

        if(_isActiveMagneticSwitchEffect) return;

        _isActiveMagneticSwitchEffect = true;
        
        if (currentMagCore != null)
        {
            var magCoreSO = currentMagCore.GetMagCoreSO();
            var currentUpgradeValue = currentMagCore.currentUpgradeValue;
            var duration = magCoreSO.magnetEffectDuration;
        
            StartCoroutine(magCoreSO.MagnetSwitchEffect(abilitySystem, currentUpgradeValue, duration,
                ()=>
                {
                    _isActiveMagneticSwitchEffect = false;
                }));
        }
    }

    public void AttackStart(int hitboxGroupId = default)
    {
        if (_currentWeapon == null)
        {
            Debug.Log("CurrentWeapon is null");
            return;
        }

        _currentWeapon.AttackStart(hitboxGroupId);
        _abilitySystem.TriggerEvent(TriggerEventType.OnAttack, _abilitySystem);
    }

    public void AttackEnd(int hitboxGroupId = default)
    {
        if (_currentWeapon == null)
        {
            Debug.Log("CurrentWeapon is null");
            return;
        }

        _currentWeapon.AttackEnd(hitboxGroupId);
    }

    public void ActivateSkill()
    {
		if (_currentWeapon == null) 
        {
		    Debug.Log("CurrentWeapon is null");
            return;
        }

        if (CurrentWeaponType == WeaponType.Bow)
        {
            var controller = GetComponent<PlayerController>();
            controller.SetForceRotationToAim();
        }

        UseSkill(_currentWeapon.OnSkill());
    }

    public void ChangePolarity()
    {
        _currentWeapon.ChangePolarity();
    }

    private void OnHitAction(HitInfo hitInfo)
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = hitInfo.collider.gameObject.GetComponent<Enemy>();
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, enemy.blackboard.abilitySystem);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, _abilitySystem);
            
            GameplayEffect damageEffect = _damageEffect.DeepCopy();
            GameplayEffect resistanceEffect = _resistanceEffect.DeepCopy();
            
            (damageEffect.amount, damageEffect.extraData.isCritical) = GameManager.Instance.Player.GetAttackDamage(_currentAttackLevel);
            damageEffect.extraData.hitInfo = hitInfo;

            enemy.blackboard.abilitySystem.ApplyEffect(damageEffect);
            enemy.blackboard.abilitySystem.ApplyEffect(resistanceEffect);
        }
    }
    
    private void UseSkill(int skillNumber)
    {
        switch (skillNumber)
        {
            case 1:
                _animator.SetInteger("SkillIndex", 1);
                _animator.SetTrigger("Skill");
                break;
            case 2:
                _animator.SetInteger("SkillIndex", 2);
                _animator.SetTrigger("Skill");
                break;
            case 3:
                //animator.SetTrigger("Skill3");
                break;
            default:
                Debug.LogWarning("Invalid Skill Number");
                break;
        }
    }

    
    public void SpawnSkillEffect(GameObject skillObj)
    {
        if(CurrentWeaponType == WeaponType.Bow)
        {
            Instantiate(skillObj, _currentWeapon.transform.position, transform.rotation);
            var arrowSfxRandomClip = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Player.Attack.ArrowE);
            AudioManager.Instance.PlaySFX(arrowSfxRandomClip);
        }
        else
        {
            Instantiate(skillObj, transform.position, transform.rotation);

            if (CurrentWeaponType == WeaponType.GreatSword)
            {
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Skill.GreatSword.Cast);
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Skill.GreatSword.Wind);
            }

            else
            {
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Skill.Hammer.Cast);
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Skill.Hammer.EnergyField);
            }
        }
    }

    public void CreateProjectile(int projectileLaunchMode)
    {
        Projectile projectile = _currentWeapon.CreateProjectile(projectileLaunchMode);
        projectile.OnHit += OnHitAction;
    }

    public void StartCharging()
    {
        VFXManager.Instance.TriggerVFX(VFXType.CHARGING_ARCHER_SKILL, _currentWeapon.transform.position, _currentWeapon.transform.rotation);
        var bowStretchSFX = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Player.Attack.BowStretch);
        AudioManager.Instance.PlaySFX(bowStretchSFX);
    }

    public Transform GetHandTransform()
    {
        return _currentWeapon.transform ? _currentWeapon.transform : transform;
    }

    public void SetAttackLevel(float damageLevel)
    {
        _currentAttackLevel = damageLevel;
    }
}
