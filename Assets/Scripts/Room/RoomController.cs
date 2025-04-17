using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var gateContollers = GetComponentsInChildren<Gate>().ToList();
        
        foreach (var gate in gateContollers)
        {
            gates[gate.roomDirection] = gate;
        }
        
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
        SceneController.Instance.EnterRoom(roomIndex, direction);
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

    public void OnPlayerEnter(RoomDirection direction)
    {
        SetGateOpen(false);
        var gateDirection = (RoomDirection)(((int)direction + 2) % 4);

        var player = GameManager.Instance.Player;

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.gameObject.transform.position = gates[gateDirection].playerSpawnPoint.position;
        player.gameObject.transform.rotation = gates[gateDirection].playerSpawnPoint.rotation;
        controller.enabled = true;
        
        gameObject.SetActive(true);
    }

    public void OnPlayerExit()
    {
        SetGateOpen(false);
        gameObject.SetActive(false);
    }
}
