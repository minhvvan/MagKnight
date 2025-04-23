using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using hvvan;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<RoomDirection, Gate> gates;

    private int roomIndex;

    public int RoomIndex => roomIndex;
    public Room Room { get; private set; }
    
    //TODO: 클리어 판정을 위한 장소에 도착하면 Invoke
    public Action OnClearSpotReached;

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

    private void OnGateEntered(RoomDirection direction)
    {
        _ = RoomSceneController.Instance.EnterRoom(roomIndex, direction);
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

    public void OnPlayerEnter(RoomDirection direction = RoomDirection.South)
    {
        SetGateOpen(false);
        var gateDirection = (RoomDirection)(((int)direction + 2) % 4);

        var player = GameManager.Instance.Player;

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.TeleportByTransform(player.gameObject, gates[gateDirection].playerSpawnPoint);
        
        gameObject.SetActive(true);
    }

    public void OnPlayerExit()
    {
        SetGateOpen(false);
        gameObject.SetActive(false);
    }

    public void Reward()
    {
        //TODO: 보상 지급
    }
}
