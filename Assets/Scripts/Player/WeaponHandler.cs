using System;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class WeaponHandler: MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;

    private WeaponPrefabSO _weaponSO;
    private BaseWeapon _currentWeapon;
    
    private async void Awake()
    {
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
}
