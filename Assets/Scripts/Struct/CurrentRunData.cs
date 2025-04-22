using System;
using UnityEngine;

[Serializable]
public class CurrentRunData : ISaveData
{
    public SaveDataType DataType => SaveDataType.Temporary;
    
    // 현재 게임 실행 정보
    public int currentFloor = 0;
    public Room currentRoom;
    public Vector3 playerPosition = Vector3.zero;
    public int seed = (int)DateTime.Now.Ticks % int.MaxValue;

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