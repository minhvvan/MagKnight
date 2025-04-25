using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// 아이템의 등급입니다.
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "ItemList", menuName = "SO/Item/ItemList")]
public class ItemListSO : ScriptableObject
{
    [Header("Probability")]
    public float healthPackDropValue; //0~1f ex. 0.03f = 3%의 확률

    [Header("ItemEffect"), Tooltip("[0]: Spawn&Stay \n [1]: OpenCrate \n [2]: CrateSpawnItem")]
    public SerializedDictionary<ItemRarity, GameObject> itemVfxPrefab;
    public SerializedDictionary<ItemRarity, List<GameObject>> lootVfxPrefab;
    
    [Header("LootCrate")]
    public GameObject lootCratePrefab;
    
    [Header("Artifact")] 
    public GameObject artifactPrefab;
    public SerializedDictionary<ItemRarity, List<ArtifactDataSO>> artifactList;
    
    [Header("MagCore")]
    public GameObject magCorePrefab;
    public SerializedDictionary<ItemRarity, List<MagCoreSO>> magCoreList;
    
    [Header("HealthPack")]
    public GameObject healthPackPrefab;
    public SerializedDictionary<ItemRarity, List<HealthPackSO>> healthPackList;

    private void OnValidate()
    {
        if(healthPackDropValue > 1.0f) healthPackDropValue = 1.0f;
        if(healthPackDropValue < 0) healthPackDropValue = 0.0f;
    }
}
