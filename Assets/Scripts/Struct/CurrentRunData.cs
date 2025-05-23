﻿using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CurrentRunData : ISaveData
{
    public SaveDataType DataType => SaveDataType.Temporary;

    public bool isDungeonEnter = false;
    
    // 현재 게임 실행 정보
    public int currentFloor = 0;
    public int currentRoomIndex;
    public Vector3 lastPlayerPosition = Vector3.zero;
    public Quaternion lastPlayerRotation = Quaternion.identity;
    public int seed = (int)DateTime.Now.Ticks % int.MaxValue;
    public List<int> clearedRooms = new List<int>();
    public int clearedRoomsCount = 0;
    public int opponentsDefeated;
    public float playTime;
    
    public PlayerStat playerStat = new();
    
    //무기
    public MagCoreSO currentMagCoreSO;
    public string currentItemName;
    public ItemCategory currentItemCategory;
    public ItemRarity currentItemRarity;
    public WeaponType currentWeapon;
    public PartsType currentPartsType;
    public int currentPartsUpgradeValue;
    public int scrap;
    
    //아티팩트
    public SerializedDictionary<int, int> leftArtifacts = new SerializedDictionary<int, int>();
    public SerializedDictionary<int, int> rightArtifacts = new SerializedDictionary<int, int>();
    public List<int> artifactsId = new List<int>();
    
    public string GetDataKey()
    {
        return Constants.CurrentRun;
    }
}