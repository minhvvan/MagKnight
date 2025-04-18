using System;
using UnityEngine;

[Serializable]
public class CurrentRunData : ISaveData
{
    public SaveDataType DataType => SaveDataType.Temporary;
    
    // 현재 게임 실행 정보
    public int currentFloor;
    public int currentRoom;
    public Vector3 playerPosition;
    public int seed;

    public CurrentRunData()
    {
        currentFloor = 0;
        currentRoom = 0;
        playerPosition = Vector3.zero;
        seed = (int)DateTime.Now.Ticks % int.MaxValue;
    }

    //TODO
    /*
     * 현재 스탯, 무기, 아티팩트, 재화
     */
    
    public string GetDataKey()
    {
        return Constants.CurrentRun;
    }
}