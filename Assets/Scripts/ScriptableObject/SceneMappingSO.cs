using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/SceneMapping", fileName = "SceneMapping")]
public class SceneMappingSO: ScriptableObject
{
    public SerializedDictionary<string, SceneData> scenes = new SerializedDictionary<string, SceneData>();
}

[Serializable]
public struct SceneData
{
    public string sceneTitle;
    
    //부가 정보 추가
}