using System;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private BaseWeapon _currentWeapon;
    private WeaponPrefabSO _weaponSO;
    private AbilitySystem _abilitySystem;
    
    public MagCore currentMagCore;
    public WeaponType CurrentWeaponType { get; private set; }
    private bool _isActiveMagneticSwitchEffect = false;
    private Animator _animator;

    private GameplayEffect _damageEffect;
    private GameplayEffect _resistanceEffect;
    
    private async void Awake()
    {
        _abilitySystem = GetComponent<AbilitySystem>();
        _animator = GetComponent<Animator>();
        
        _damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, 0)
        {
            sourceTransform = transform
        };
        _resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.ResistanceDamage, 10)
        {
            sourceTransform = transform
        };
        
        try
        {
            _weaponSO =
                await DataManager.Instance.LoadScriptableObjectAsync<WeaponPrefabSO>(Addresses.Data.Weapon.Katana);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void SetCurrentWeapon(WeaponType weaponType)
    {
        if (_weaponSO == null)
        {
            Debug.Log("WeaponData is null");
            return;
        }

        CurrentWeaponType = weaponType;
        if (CurrentWeaponType == WeaponType.None)
        {
            Debug.Log("WeaponType is null");
            return;
        }

        if (_currentWeapon != null) Destroy(_currentWeapon.gameObject);
        _currentWeapon = Instantiate(_weaponSO.weapons[weaponType], weaponSocket).GetComponent<BaseWeapon>();
        _currentWeapon.OnHit += OnHitAction;
    }

    public void DropPrevWeapon(Transform newCorePos)
    {
        var prevWeaponCategory = currentMagCore.category;
        var prevWeaponRarity = currentMagCore.rarity;
        var dropObj =
            ItemManager.Instance.CreateItem(prevWeaponCategory, prevWeaponRarity, newCorePos.position,
                Quaternion.identity);

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

    //착용중인 파츠의 레벨 업그레이드
    public void UpgradeCurrentParts()
    {
        if(currentMagCore != null) currentMagCore.Upgrade(_abilitySystem);
    }
    
    //극성 전환 효과 활성화
    public void ActivateMagnetSwitchEffect(AbilitySystem abilitySystem, MagneticType type)
    {
        var vfxObj = Instantiate(ItemManager.Instance.magnetSwitchVfxPrefab, _currentWeapon.transform);
        //vfxObj.transform.localScale *= 2f;
        var vfxs = vfxObj.GetComponentsInChildren<ParticleSystem>();
        foreach (var vfx in vfxs)
        {
            var main = vfx.main;
            if(type == MagneticType.N) main.startColor = Color.red;
            else if(type == MagneticType.S) main.startColor = Color.blue;
        }
        Destroy(vfxObj, 0.25f);
        
        switch (_isActiveMagneticSwitchEffect)
        {
            case true:
                return;
            case false:
                _isActiveMagneticSwitchEffect = true;
                break;
        }
        
        var magCoreSO = currentMagCore.GetMagCoreSO();
        var currentUpgradeValue = currentMagCore.currentUpgradeValue;
        var duration = magCoreSO.magnetEffectDuration;
        
        StartCoroutine(magCoreSO.MagnetSwitchEffect(abilitySystem, currentUpgradeValue, duration,
            ()=>
            {
                _isActiveMagneticSwitchEffect = false;
            }));
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
            
            _damageEffect.amount = GameManager.Instance.Player.GetAttackDamage();
            
            enemy.blackboard.abilitySystem.ApplyEffect(_damageEffect);
            enemy.blackboard.abilitySystem.ApplyEffect(_resistanceEffect);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, enemy.blackboard.abilitySystem);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, _abilitySystem);
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
        Instantiate(skillObj, transform.position, transform.rotation);
    }
    
    public void CreateProjectile(GameObject prefab)
    {
        Projectile projectile = _currentWeapon.CreateProjectile(prefab);
        projectile.OnHit += OnHitAction;
    }
}
