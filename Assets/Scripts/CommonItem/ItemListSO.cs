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
}
