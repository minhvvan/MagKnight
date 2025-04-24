using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using hvvan;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour, IObserver<bool>
{
    [SerializeField] private SerializedDictionary<RoomDirection, Gate> gates;
    [SerializeField] private ClearRoomField clearRoomField;

    private int _roomIndex;
    private bool _cleared = false;

    public int RoomIndex => _roomIndex;
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
        
        //클리어 필드 비활성화
        SetClearField(false);
    }

    private void Start()
    {
        if (clearRoomField != null)
        {
            clearRoomField.Subscribe(this);
        }
        
        foreach (var gate in gates.Values)
        {
            gate.OnEnter += OnGateEntered;
        }
    }

    private void OnGateEntered(RoomDirection direction)
    {
        _ = RoomSceneController.Instance.EnterRoom(_roomIndex, direction);
    }

    public void SetRoomData(Room roomData, int index)
    {
        _roomIndex = index;
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
        SetClearField(true);
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

    public void SetClearField(bool isEnable)
    {
        if (clearRoomField)
        {
            clearRoomField.gameObject.SetActive(isEnable);
        }
    }
    
    public void OnNext(bool reached)
    {
        if (reached && !_cleared)
        {
            _cleared = true;
            Reward();
            GameManager.Instance.ChangeGameState(GameState.RoomClear);
        }
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }
}
