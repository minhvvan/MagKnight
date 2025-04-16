using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<RoomDirection, Gate> gates;

    private int roomIndex;

    public Room Room { get; private set; }

    private void Awake()
    {
        //모든 문 비활성화
        foreach (var gate in gates.Values)
        {
            gate.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        foreach (var gate in gates.Values)
        {
            gate.OnEnter += OnGateEntered;
        }
    }

    private async void OnGateEntered(RoomDirection direction)
    {
        SceneController.Instance.EnterRoom(roomIndex, Room.connectedRooms[(int)direction]);
    }

    public void SetRoomData(Room roomData, int index)
    {
        roomIndex = index;
        Room = roomData;
    }

    public void SetGateOpen(bool isOpen)
    {
        //연결이 된 gate만 제어
        for (var dir = RoomDirection.East; dir < RoomDirection.Max; dir++)
        {
            if(Room.connectedRooms[(int)dir] == Room.Empty || Room.connectedRooms[(int)dir] == Room.Blocked) continue; 
            gates[dir].gameObject.SetActive(isOpen);
        }
    }
}
