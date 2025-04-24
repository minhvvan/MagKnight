using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 아이템의 분류입니다.
/// </summary>
public enum ItemCategory
{
    Artifact,
    MagCore,
    HealthPotion
}

public class ItemManager : Singleton<ItemManager>
{
    public Dictionary<ItemRarity, List<ArtifactDataSO>> artifactList = new Dictionary<ItemRarity, List<ArtifactDataSO>>();
    public Dictionary<ItemRarity, List<MagCoreSO>> magCoreList = new Dictionary<ItemRarity, List<MagCoreSO>>();
    public Dictionary<ItemRarity, List<HealthPackSO>> healthPackList = new Dictionary<ItemRarity, List<HealthPackSO>>();
    
    private GameObject _artifactPrefab;
    private GameObject _magCorePrefab;
    private GameObject _healthPackPrefab;
    private GameObject _lootCratePrefab;
    
    public bool IsInitialized { get; private set; } = false;

    private async void Awake()
    {
       await Initialize();
    }

    private void Update()
    {
        //테스트용
        if (Input.GetKeyDown(KeyCode.H))
        {
            CreateItem(ItemCategory.Artifact, ItemRarity.Common, transform.position, Quaternion.identity);
            CreateItem(ItemCategory.MagCore, ItemRarity.Common, transform.position += Vector3.left, Quaternion.identity);
            CreateItem(ItemCategory.HealthPotion, ItemRarity.Common, transform.position += Vector3.right, Quaternion.identity);
        }
    }
    
    public async UniTask Initialize()
    {
        IsInitialized = true;
        await SetAllItemUpdate();
    }

    /// <summary>
    /// 모든 아이템 리스트를 갱신, 로드 합니다.
    /// </summary>
    public async UniTask SetAllItemUpdate()
    {
        var datas = await DataManager.Instance.LoadDataAsync<ItemListSO>(Addresses.Data.Item.ItemListData);

        _lootCratePrefab = datas.lootCratePrefab;
        
        _artifactPrefab = datas.artifactPrefab;
        artifactList?.Clear();
        foreach (var data in datas.artifactList)
        {
            artifactList?.Add(data.Key, data.Value);
        }

        _magCorePrefab = datas.magCorePrefab;
        magCoreList?.Clear();
        foreach (var data in datas.magCoreList)
        {
            magCoreList?.Add(data.Key, data.Value);
        }

        _healthPackPrefab = datas.healthPackPrefab;
        healthPackList?.Clear();
        foreach (var data in datas.healthPackList)
        {
            healthPackList?.Add(data.Key, data.Value);
        }
    }
    
    
    
    /// <summary>
    /// 원하는 위치에 아이템을 생성합니다. 기본적으로 해당 범주 내의 아이템을 랜덤하게 생성합니다.
    /// </summary>
    /// <param name="category">아이템의 분류를 지정해주세요.</param>
    /// <param name="rarity">아이템의 등급을 지정해주세요.</param>
    /// <param name="position">생성할 위치를 지정해주세요.</param>
    /// <param name="rotation">생성한 아이템 오브젝트의 회전값을 지정해주세요.</param>
    /// <param name="parent">(Option)Parent지정이 필요한 경우 지정해주세요.</param>
    /// <param name="itemName">(Option)원하는 아이템을 지정하여 생성하고 싶다면 아이템명을 입력해 받습니다.</param>
    /// <returns>생성한 아이템을 반환합니다.</returns>
    public GameObject CreateItem(ItemCategory category, ItemRarity rarity, Vector3 position, Quaternion rotation, 
        [CanBeNull] Transform parent = null, [CanBeNull] string itemName = null)
    {
        bool isRandom = itemName == null;
        int randomIndex = 0;
        
        switch (category)
        {
            case ItemCategory.Artifact:
                if (_artifactPrefab == null) break;
                if(isRandom) randomIndex = Random.Range(0, artifactList[rarity].Count-1);
                
                ArtifactDataSO artifactData;
                
                //아이템명을 지정하여 생성한 경우.
                if (itemName != null)
                {
                    if (artifactList[rarity].FirstOrDefault(data => data.itemName == itemName) 
                            is var containData && containData != null)
                    {
                        artifactData = containData;
                    }
                    else//해당 범주 내 아이템이 없음.
                    {
                        Debug.LogError($"Item {itemName} not found");
                        break;
                    }
                }
                else//범주 내 랜덤 생성인 경우.
                {
                    if (artifactList[rarity][randomIndex] is var containData && containData != null)
                    {
                        artifactData = containData;
                    }
                    else
                    {
                        Debug.LogError($"this {category}-{rarity} Item not found");
                        break;
                    }
                }

                var artifactObj = Instantiate(_artifactPrefab, position, rotation, parent);
                var artifact = artifactObj.GetComponent<ArtifactObject>();
                artifact.SetArtifactData(artifactData);
                
                return artifactObj;
            
            case ItemCategory.MagCore:
                if (_magCorePrefab == null) break;
                if(isRandom) randomIndex = Random.Range(0, magCoreList[rarity].Count-1);
                
                MagCoreSO magCoreData;
                
                //아이템명을 지정하여 생성한 경우.
                if (itemName != null)
                {
                    if (magCoreList[rarity].FirstOrDefault(data => data.itemName == itemName) 
                            is var containData && containData != null)
                    {
                        magCoreData = containData;
                    }
                    else//해당 범주 내 아이템이 없음.
                    {
                        Debug.LogError($"Item {itemName} not found");
                        break;
                    }
                }
                else//범주 내 랜덤 생성인 경우.
                {
                    if (magCoreList[rarity][randomIndex] is var containData && containData != null)
                    {
                        magCoreData = containData;
                    }
                    else
                    {
                        Debug.LogError($"this {category}-{rarity} Item not found");
                        break;
                    }
                }
                
                var magCoreObj = Instantiate(_magCorePrefab, position, rotation, parent);
                var magCore = magCoreObj.GetComponent<MagCore>();
                magCore.SetMagCoreData(magCoreData);
                
                return magCoreObj;
            
            case ItemCategory.HealthPotion:
                if (_healthPackPrefab == null) break;
                if(isRandom) randomIndex = Random.Range(0, healthPackList[rarity].Count-1);
                
                HealthPackSO healthPotionData;
                
                //아이템명을 지정하여 생성한 경우.
                if (itemName != null)
                {
                    if (healthPackList[rarity].FirstOrDefault(data => data.itemName == itemName) 
                            is var containData && containData != null)
                    {
                        healthPotionData = containData;
                    }
                    else//해당 범주 내 아이템이 없음.
                    {
                        Debug.LogError($"Item {itemName} not found");
                        break;
                    }
                }
                else//범주 내 랜덤 생성인 경우.
                {
                    if (healthPackList[rarity][randomIndex] is var containData && containData != null)
                    {
                        healthPotionData = containData;
                    }
                    else
                    {
                        Debug.LogError($"this {category}-{rarity} Item not found");
                        break;
                    }
                }
                
                var healthPotionObj = Instantiate(_healthPackPrefab, position, rotation, parent);
                var healthPotion = healthPotionObj.GetComponent<HealthPack>();
                healthPotion.SetHealthPotionData(healthPotionData);
                
                return healthPotionObj;
        }
        Debug.LogError($"Cannot found item / Path: {category}, {rarity}");
        return null;
    }

    //지정한 등급의 상자 생성.
    public void SpawnLootCrate(ItemCategory category, ItemRarity rarity, Vector3 position, Quaternion rotation)
    {
        var obj = Instantiate(_lootCratePrefab, position, rotation);
        var lootCrate = obj.GetComponent<LootCrate>();
        lootCrate.SetLootCrate(category, rarity);
    }
}
