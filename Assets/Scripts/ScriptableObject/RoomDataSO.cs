using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Room/RoomList", fileName = "RoomData")]
public class RoomDataSO: ScriptableObject
{
    public SerializedDictionary<RoomType, Room> rooms = new SerializedDictionary<RoomType, Room>();
    public SerializedDictionary<RoomType, Limit> roomNumLimit = new SerializedDictionary<RoomType, Limit>();
}

[Serializable]
public struct Limit
{
    public int min;
    public int max;
}