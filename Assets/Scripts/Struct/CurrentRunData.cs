using System;
using System.Collections.Generic;
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
    
    public bool artifactSupplied = false;

    public PlayerStat playerStat = new();
    
    //무기
    public WeaponType currentWeapon;
    public PartsType currentPartsType;
    public int currentPartsUpgradeValue;
    public int scrap;
    
    //아티팩트
    public List<int> leftArtifacts = new List<int>();
    public List<int> rightArtifacts = new List<int>();
    
    public string GetDataKey()
    {
        return Constants.CurrentRun;
    }
}