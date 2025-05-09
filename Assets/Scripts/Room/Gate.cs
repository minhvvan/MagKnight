using System;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public Transform playerSpawnPoint;
    
    public RoomDirection roomDirection;
    public Action<RoomDirection> OnEnter;
    public Transform indicatorPoint;

    void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(roomDirection);
    }
}
