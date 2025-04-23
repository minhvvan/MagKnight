using System;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class WeaponHandler: MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;

    private WeaponPrefabSO _weaponSO;
    private BaseWeapon _currentWeapon;
    private AbilitySystem _abilitySystem;
    
    private async void Awake()
    {
        _abilitySystem = GetComponent<AbilitySystem>();
        
        try
        {
            _weaponSO = await DataManager.Instance.LoadDataAsync<WeaponPrefabSO>(Addresses.Data.Weapon.Katana);
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

        _currentWeapon = Instantiate(_weaponSO.weapons[weaponType], weaponSocket).GetComponent<BaseWeapon>();
        _currentWeapon.OnHit += OnHitAction;
    }

    public void AttackStart()
    {
        if (_currentWeapon == null)
        {
            Debug.Log("CurrentWeapon is null");
            return;
        }

        _currentWeapon.AttackStart();
        _abilitySystem.TriggerEvent(TriggerEventType.OnAttack, _abilitySystem);
    }

    public void AttackEnd()
    {
        if (_currentWeapon == null)
        {
            Debug.Log("CurrentWeapon is null");
            return;
        }
        
        _currentWeapon.AttackEnd();
    }

    public void ActivateSkill()
    {
        
    }

    public void ChangePolarity()
    {
        _currentWeapon.ChangePolarity();
    }

    private void OnHitAction(HitInfo hitInfo)
    {
        if(hitInfo.hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var damage = _abilitySystem.GetValue(AttributeType.Strength);
            Enemy enemy = hitInfo.hit.collider.gameObject.GetComponent<Enemy>();
            GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, damage);
            GameplayEffect resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.Resistance, damage);
            enemy.blackboard.abilitySystem.ApplyEffect(damageEffect);
            enemy.blackboard.abilitySystem.ApplyEffect(resistanceEffect);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, enemy.blackboard.abilitySystem);
        }
    }
}
