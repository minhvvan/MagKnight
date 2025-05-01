using System;
using hvvan;
using UnityEngine;

public enum SupplyType
{
    None,
    Weapon,
    Artifact,
    Max
}
public class BaseCampSupplyChecker: MonoBehaviour
{
    [SerializeField] private SupplyType supplyType = SupplyType.None;

    private LootCrate _lootCrate;
    private bool _isCheckComplete = false;
    
    private void Awake()
    {
        _lootCrate = GetComponent<LootCrate>();
    }

    private void Update()
    {
        if(_isCheckComplete || GameManager.Instance.CurrentGameState != GameState.BaseCamp) return;
        CheckSupply();
    }

    private void CheckSupply()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        if (currentRunData == null) return;

        _isCheckComplete = true;
        
        if (supplyType == SupplyType.Artifact)
        {
            //아티팩트를 이미 받았다면 상자 비활성화
            if (currentRunData.leftArtifacts.Count != 0 || currentRunData.rightArtifacts.Count != 0)
            {
                //상자 비활성
                _lootCrate.SetDisable();
            }
        }
        else
        {
            //무기를 이미 받았다면 상자 비활성화
            if (currentRunData.currentWeapon != WeaponType.None)
            {
                //상자 비활성
                _lootCrate.SetDisable();
            }
        }
    }
}
