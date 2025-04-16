using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public RoomDirection roomDirection;
    public Action<RoomDirection> OnEnter;

    void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(roomDirection);
    }
}
