using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CurrentRunData : ISaveData
{
    public SaveDataType DataType => SaveDataType.Temporary;
    
    // 현재 게임 실행 정보
    public int currentFloor = 0;
    public int currentRoomIndex;
    public Vector3 lastPlayerPosition = Vector3.zero;
    public Quaternion lastPlayerRotation = Quaternion.identity;
    public int seed = (int)DateTime.Now.Ticks % int.MaxValue;
    
    public List<int> clearedRooms = new List<int>();

    //TODO
    /*
     * 현재 스탯, 무기, 아티팩트, 재화
     */
    public PlayerStat playerStat = new();
    public WeaponType currentWeapon;
    
    public string GetDataKey()
    {
        return Constants.CurrentRun;
    }
}