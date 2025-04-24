using System;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class WeaponHandler: MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;

    private WeaponPrefabSO _weaponSO;
    private BaseWeapon _currentWeapon;
    public WeaponType CurrentWeaponType { get; private set; }

    private async void Awake()
    {
        try
        {
            _weaponSO = await DataManager.Instance.LoadScriptableObjectAsync<WeaponPrefabSO>(Addresses.Data.Weapon.Katana);
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
        _currentWeapon = Instantiate(_weaponSO.weapons[weaponType], weaponSocket).GetComponent<BaseWeapon>();
    }
    
    public void AttackStart()
    {
        if (_currentWeapon == null)
        {
            Debug.Log("CurrentWeapon is null");
            return;
        }

        _currentWeapon.AttackStart();
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
}
