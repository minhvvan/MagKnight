using System;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;

    private WeaponPrefabSO _weaponSO;
    [SerializeField] private BaseWeapon _currentWeapon;
    private AbilitySystem _abilitySystem;
    public MagCore currentMagCore;
    public WeaponType CurrentWeaponType { get; private set; }

    private async void Awake()
    {
        _abilitySystem = GetComponent<AbilitySystem>();

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
        var vfxObj = Instantiate(ItemManager.Instance.weaponChangeVfxPrefab, _currentWeapon.transform);
        vfxObj.transform.localScale *= 0.5f;
        vfxObj.transform.rotation *= Quaternion.Euler(90f,0, 0);
        Destroy(vfxObj, 0.25f);
        
        Destroy(currentMagCore.gameObject);
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
        if (hitInfo.hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var damage = _abilitySystem.GetValue(AttributeType.Strength);
            Enemy enemy = hitInfo.hit.collider.gameObject.GetComponent<Enemy>();
            GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, damage);
            damageEffect.sourceTransform = transform;
            GameplayEffect resistanceEffect =
                new GameplayEffect(EffectType.Instant, AttributeType.ResistanceDamage, damage);
            enemy.blackboard.abilitySystem.ApplyEffect(damageEffect);
            enemy.blackboard.abilitySystem.ApplyEffect(resistanceEffect);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, enemy.blackboard.abilitySystem);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, _abilitySystem);
        }
    }
}
