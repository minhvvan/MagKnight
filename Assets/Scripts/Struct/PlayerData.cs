using System;
using Managers;

[Serializable]
public class PlayerData: ISaveData
{
    public SaveDataType DataType => SaveDataType.Permanent;

    public PlayerStat PlayerStat;
    public int Currency;
    
    //lock된 아티팩트 등 추가 가능
    public string GetDataKey()
    {
        return Constants.PlayerData;
    }
}
