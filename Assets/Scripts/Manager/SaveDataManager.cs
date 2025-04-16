using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SaveDataType
{
    Permanent,  // 영구 저장 데이터 (게임 종료 후에도 유지)
    Temporary   // 일시 저장 데이터 (게임 세션 동안만 유지)
}

public interface ISaveData
{
    SaveDataType DataType { get; }
    string GetDataKey();
}

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

public class SaveDataManager: Singleton<SaveDataManager>
{
    private Dictionary<string, ISaveData> _saveData = new Dictionary<string, ISaveData>();
    private string SavePath => Path.Combine(Application.persistentDataPath, "SaveData");
    
    protected override void Initialize()
    {
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }
    }

    public T LoadData<T>(string key) where T : ISaveData, new()
    {
        if (_saveData.TryGetValue(key, out ISaveData cachedData))
        {
            if (cachedData is T typedData)
            {
                return typedData;
            }
        }
        
        T data = new T();
        
        string filePath = Path.Combine(SavePath, $"{key}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<T>(json);
        }
        
        _saveData[key] = data;
        return data;
    }

    public void SaveData(string key, ISaveData data)
    {
        _saveData[key] = data;
        
        string filePath = Path.Combine(SavePath, $"{key}.json");
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }
}
