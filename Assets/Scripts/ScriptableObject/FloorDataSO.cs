using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Room/Floor", fileName = "FloorData")]
public class FloorDataSO: ScriptableObject
{
    public List<RoomDataSO> Floor = new List<RoomDataSO>();
}
