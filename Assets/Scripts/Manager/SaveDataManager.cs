using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        // 캐시된 데이터 확인
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
            try
            {
                string json;
                
                // OS 확인 후 다른 방식으로 파일 로드
                if (IsMacOS())
                {
                    // macOS용 로드 방식 - FileShare.ReadWrite 사용
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }
                }
                else
                {
                    // Windows 및 기타 OS용 로드 방식
                    json = File.ReadAllText(filePath);
                }
                
                var data = JsonUtility.FromJson<T>(json);
                _saveData[key] = data; // 기존 데이터가 있을 경우 덮어쓰기
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"파일 로드 중 오류 발생: {e.Message}");
                return null;
            }
        }
        
        return null;
    }

    public async UniTask SaveData(string key, ISaveData data)
    {
        _saveData[key] = data;
        
        string filePath = Path.Combine(SavePath, $"{key}.json");
        string json = JsonUtility.ToJson(data);
        
        try
        {
            // OS 확인 후 다른 방식으로 파일 저장
            if (IsMacOS())
            {
                // macOS용 저장 방식
                // 임시 파일에 먼저 쓰고 이동하는 방식 사용
                string tempFilePath = Path.Combine(SavePath, $"{key}_temp.json");
                
                // 임시 파일에 데이터 쓰기
                using (FileStream fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    await writer.WriteAsync(json);
                    await writer.FlushAsync();
                }
                
                // 기존 파일이 있으면 삭제
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                // 임시 파일을 실제 위치로 이동
                File.Move(tempFilePath, filePath);
            }
            else
            {
                // Windows 및 기타 OS용 저장 방식
                await File.WriteAllTextAsync(filePath, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 저장 중 오류 발생: {e.Message}");
            
            // 최후의 수단: 동기 방식 시도
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (Exception e2)
            {
                Debug.LogError($"동기 파일 저장 중 오류 발생: {e2.Message}");
            }
        }
    }

    public void DeleteData(string key)
    {
        _saveData.Remove(key);
        string filePath = Path.Combine(SavePath, $"{key}.json");
        
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 삭제 중 오류 발생: {e.Message}");
        }
    }

    public T CreateData<T>(string key) where T: ISaveData, new()
    {
        var data = new T();
        _saveData[key] = data;
        return data;
    }
    
    // 현재 플랫폼이 macOS인지 확인하는 헬퍼 메서드
    private bool IsMacOS()
    {
        return Application.platform == RuntimePlatform.OSXEditor || 
               Application.platform == RuntimePlatform.OSXPlayer;
    }
}