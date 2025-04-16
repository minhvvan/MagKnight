public interface ISaveData
{
    SaveDataType DataType { get; }
    string GetDataKey();
}