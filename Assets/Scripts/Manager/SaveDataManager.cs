using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SaveDataType
{
    Permanent,  // 영구 저장 데이터 (게임 종료 후에도 유지)
    Temporary   // 일시 저장 데이터 (게임 세션 동안만 유지)
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

    public T LoadData<T>(string key) where T : class, ISaveData
    {
        if (_saveData.TryGetValue(key, out ISaveData cachedData))
        {
            if (cachedData is T typedData)
            {
                return typedData;
            }
        }
        
        string filePath = Path.Combine(SavePath, $"{key}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var data = JsonUtility.FromJson<T>(json);
            _saveData.Add(key, data);
            
            return data;
        }
        
        return null;
    }

    public void SaveData(string key, ISaveData data)
    {
        _saveData[key] = data;
        
        string filePath = Path.Combine(SavePath, $"{key}.json");
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public void DeleteData(string key)
    {
        _saveData.Remove(key);
        string filePath = Path.Combine(SavePath, $"{key}.json");
        File.Delete(filePath);
    }
}
