
public class PlayerData: ISaveData
{
    public SaveDataType DataType => SaveDataType.Permanent;
    
    //TODO: 영구 스탯, 재화 관리
    //lock된 아티팩트 등 추가 가능
    
    public string GetDataKey()
    {
        return Constants.PlayerData;
    }
}
