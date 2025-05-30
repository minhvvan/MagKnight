using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 아이템의 분류입니다.
/// </summary>
public enum ItemCategory
{
    Artifact,
    MagCore,
    HealthPack
}

public class ItemManager : Singleton<ItemManager>
{
    public SerializedDictionary<ItemRarity, List<ArtifactDataSO>> artifactList = new  SerializedDictionary<ItemRarity, List<ArtifactDataSO>>();
    public SerializedDictionary<ItemRarity, List<MagCoreSO>> magCoreList = new SerializedDictionary<ItemRarity, List<MagCoreSO>>();
    public SerializedDictionary<ItemRarity, List<HealthPackSO>> healthPackList = new SerializedDictionary<ItemRarity, List<HealthPackSO>>();
    
    //아이템 프리팹
    private GameObject _artifactPrefab; 
    private GameObject _magCorePrefab;
    private GameObject _healthPackPrefab;
    private GameObject _lootCratePrefab;
    
    //VFX 프리팹
    private Dictionary<ItemRarity, GameObject> _itemVfxPrefabs = new Dictionary<ItemRarity, GameObject>();
    private Dictionary<ItemRarity, List<GameObject>> _lootVfxPrefabs = new Dictionary<ItemRarity, List<GameObject>>();

    public GameObject weaponChangeVfxPrefab;
    public GameObject dismantleVfxPrefab;
    public GameObject magnetSwitchVfxPrefab;
    
    //확률값
    private float _healthPackDropValue;
    
    public bool IsInitialized { get; private set; } = false;

    private void Update()
    {
        //테스트용
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            // //죽으면 일정 확률로 힐팩 생성
            // if(CheckProbability(ItemCategory.HealthPack, ItemRarity.Common))
            // {
            //     Debug.Log("Good Luck.");
            //     CreateItem(ItemCategory.HealthPack, ItemRarity.Common, 
            //         transform.position + Vector3.up, Quaternion.identity);
            // }
            //CreateItem(ItemCategory.Artifact, ItemRarity.Common, transform.position, Quaternion.identity);
            CreateItem(ItemCategory.MagCore, ItemRarity.Epic, transform.position + Vector3.left*2f, Quaternion.identity);
            // CreateItem(ItemCategory.MagCore, ItemRarity.Common, transform.position + Vector3.left*2f, Quaternion.identity, itemName:"Bow");
            //CreateItem(ItemCategory.HealthPack, ItemRarity.Common, transform.position += Vector3.right*2f, Quaternion.identity);
            
            SpawnLootCrate(ItemCategory.Artifact, ItemRarity.Common, transform.position += Vector3.forward*3f , Quaternion.identity);
        }
    }

    protected override async void Initialize()
    {
        IsInitialized = true;
    }
    
    /// <summary>
    /// 모든 아이템 리스트를 갱신, 로드 합니다.
    /// </summary>
    public async UniTask SetAllItemUpdate(CurrentRunData currentRunData)
    {
        var datas = await DataManager.Instance.LoadData<ItemListSO>(Addresses.Data.Item.ItemListData);

        _lootCratePrefab = datas.lootCratePrefab;
        
        _itemVfxPrefabs = datas.itemVfxPrefab;
        _lootVfxPrefabs = datas.lootVfxPrefab;

        weaponChangeVfxPrefab = datas.weaponChangeVfxPrefab;
        dismantleVfxPrefab = datas.dismantleVfxPrefab;
        magnetSwitchVfxPrefab = datas.magnetSwitchVfxPrefab;
        
        _artifactPrefab = datas.artifactPrefab;
        artifactList?.Clear();

        foreach (var rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            artifactList?.Add((ItemRarity)rarity, new List<ArtifactDataSO>());
        }
        
        foreach (var data in datas.artifactMapping.artifacts.Values)
        {
            if (artifactList != null) artifactList[data.rarity].Add(data);
        }

        _magCorePrefab = datas.magCorePrefab;
        magCoreList?.Clear();
        foreach (var data in datas.magCoreList)
        {
            magCoreList?.Add(data.Key, data.Value);
        }

        _healthPackPrefab = datas.healthPackPrefab;
        _healthPackDropValue = datas.healthPackDropValue;
        healthPackList?.Clear();
        foreach (var data in datas.healthPackList)
        {
            healthPackList?.Add(data.Key, data.Value);
        }

        foreach (var artifactId in currentRunData.artifactsId)
        {
            foreach (var artifactListValue in artifactList.Values)
            {
                if (artifactListValue.FirstOrDefault(data => data.itemID == artifactId)
                        is var containData && containData != null)
                {
                    artifactListValue.Remove(containData);
                }
            }
        }
    }

    public void RemoveArtifactList(ArtifactDataSO artifactData)
    {
        foreach (var artifactListValue in artifactList.Values)
        {
            if (artifactListValue.FirstOrDefault(data => data == artifactData)
                    is var containData && containData != null)
            {
                artifactListValue.Remove(containData);
            }
        }
    }

    public void AddArtifact(ArtifactDataSO artifactData)
    {
        artifactList[artifactData.rarity]?.Add(artifactData);
    }

    /// <summary>
    /// 카테고리와 등급을 입력하면 지정된 확률을 연산해 결과를 전달합니다.
    /// </summary>
    /// <param name="category">아이템의 분류</param>
    /// <param name="rarity">아이템의 등급</param>
    /// <returns></returns>
    public bool CheckProbability(ItemCategory category, ItemRarity rarity)
    {
        switch (category)
        {
            case ItemCategory.Artifact:
                switch (rarity)
                {
                    case ItemRarity.Common:
                        break;
                }
                break;
            
            case ItemCategory.MagCore:
                switch (rarity)
                {
                    case ItemRarity.Common:
                        break;
                }
                break;
            
            case ItemCategory.HealthPack:
                switch (rarity)
                {
                    case ItemRarity.Common:
                        var result = Random.value < _healthPackDropValue;
                        return result;
                }
                break;
        }
        
        return false;
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
                if(artifactList[rarity].Count == 0) break;
                if(isRandom) randomIndex = Random.Range(0, artifactList[rarity].Count);
                
                
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
                
                if (!parent && RoomSceneController.Instance.CurrentRoomController != null)
                {
                    parent = RoomSceneController.Instance.CurrentRoomController.transform;
                }
                var artifactObj = Instantiate(_artifactPrefab, position, rotation, parent);
                var artifact = artifactObj.GetComponent<ArtifactObject>();
                artifact.SetArtifactData(artifactData);
                artifact.SetItemClass((category, rarity));
                var artifactVfx  = Instantiate(_itemVfxPrefabs[rarity], position, rotation, artifactObj.transform);
                artifactVfx.transform.localScale *= 0.5f;
                
                return artifactObj;
            
            case ItemCategory.MagCore:
                if (_magCorePrefab == null) break;
                if(isRandom) randomIndex = Random.Range(0, magCoreList[rarity].Count);
                
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
                
                if (!parent && RoomSceneController.Instance.CurrentRoomController != null)
                {
                    parent = RoomSceneController.Instance.CurrentRoomController.transform;
                }
                var magCoreObj = Instantiate(_magCorePrefab, position, rotation, parent);
                var magCore = magCoreObj.GetComponent<MagCore>();
                magCore.SetMagCoreData(magCoreData);
                magCore.SetItemClass((category, rarity));
                var magCoreVfx  = Instantiate(_itemVfxPrefabs[rarity], position, rotation, magCoreObj.transform);
                magCoreVfx.transform.localScale *= 0.5f;
                
                return magCoreObj;
            
            case ItemCategory.HealthPack:
                if (_healthPackPrefab == null) break;
                if(isRandom) randomIndex = Random.Range(0, healthPackList[rarity].Count);
                
                HealthPackSO healthPackData;
                
                //아이템명을 지정하여 생성한 경우.
                if (itemName != null)
                {
                    if (healthPackList[rarity].FirstOrDefault(data => data.itemName == itemName) 
                            is var containData && containData != null)
                    {
                        healthPackData = containData;
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
                        healthPackData = containData;
                    }
                    else
                    {
                        Debug.LogError($"this {category}-{rarity} Item not found");
                        break;
                    }
                }
                
                if (!parent && RoomSceneController.Instance.CurrentRoomController != null)
                {
                    parent = RoomSceneController.Instance.CurrentRoomController.transform;
                }
                var healthPackObj = Instantiate(_healthPackPrefab, position, rotation, parent);
                var healthPack = healthPackObj.GetComponent<HealthPack>();
                healthPack.SetHealthPotionData(healthPackData);
                healthPack.SetItemClass((category, rarity));
                var healthPackVfx  = Instantiate(_itemVfxPrefabs[rarity], position, rotation, healthPackObj.transform);
                healthPackVfx.transform.localScale *= 0.5f;
                
                return healthPackObj;
        }
        Debug.Log($"Cannot found item / Path: {category}, {rarity}");

        AudioManager.Instance.PlaySFX(AudioBase.SFX.UI.FieldItem.DropItem);
        return null;
    }

    //지정한 등급의 상자 생성.
    public void SpawnLootCrate(ItemCategory category, ItemRarity rarity, Vector3 position, Quaternion rotation, bool isBoss = false)
    {
        var currentRoom = RoomSceneController.Instance.CurrentRoomController;
        
        var obj = Instantiate(_lootCratePrefab, position, rotation, 
            currentRoom != null ? currentRoom.transform : null);
        var lootCrate = obj.GetComponent<LootCrate>();
        lootCrate.SetLootCrate(category, rarity, isBoss);
        lootCrate.rarityVfxObjects = _lootVfxPrefabs;
        
    }
}
