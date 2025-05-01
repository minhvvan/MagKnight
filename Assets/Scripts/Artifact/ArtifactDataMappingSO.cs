using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ArtifactDataMappingSO", menuName = "SO/Artifact/ArtifactDataMappingSO")]
public class ArtifactDataMappingSO : ScriptableObject
{
   public SerializedDictionary<int, ArtifactDataSO> artifacts = new SerializedDictionary<int, ArtifactDataSO>();
}
