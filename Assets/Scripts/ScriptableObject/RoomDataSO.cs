using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Room/RoomList", fileName = "RoomData")]
public class RoomDataSO: ScriptableObject
{
    public SerializedDictionary<RoomType, List<Room>> rooms = new SerializedDictionary<RoomType, List<Room>>();
    public SerializedDictionary<RoomType, Limit> roomNumLimit = new SerializedDictionary<RoomType, Limit>();
}

[Serializable]
public struct Limit
{
    public int min;
    public int max;
}