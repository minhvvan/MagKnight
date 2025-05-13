using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public enum GateSignType
{
    None,
    Cleared,
    Uncleared,
    Boss,
    Max
}

public class Gate : MonoBehaviour
{
    public Transform playerSpawnPoint;
    
    public RoomDirection roomDirection;
    public Action<RoomDirection> OnEnter;
    public Transform indicatorPoint;
    public SerializedDictionary<GateSignType, GameObject> gateSigns = new();

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        OnEnter?.Invoke(roomDirection);
    }

    public void ActiveGateSign(RoomType connectRoomRoomType, bool cleared)
    {
        GateSignType activeSign = GateSignType.None;
        
        if (connectRoomRoomType == RoomType.BossRoom)
        {
            activeSign = GateSignType.Boss;
        }
        else
        {
            activeSign = cleared ? GateSignType.Cleared : GateSignType.Uncleared;
        }
        
        foreach (var (type, sign) in gateSigns)
        {
            sign.SetActive(type == activeSign);
        }
    }
}
