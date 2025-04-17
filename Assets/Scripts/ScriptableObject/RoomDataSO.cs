using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Room", fileName = "RoomData")]
public class RoomDataSO: ScriptableObject
{
    public SerializedDictionary<RoomType, Room> rooms = new SerializedDictionary<RoomType, Room>();
}
