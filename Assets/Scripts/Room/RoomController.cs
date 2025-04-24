using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour, IObserver<bool>
{
    [SerializeField] private SerializedDictionary<RoomDirection, Gate> gates;
    [SerializeField] private ClearRoomField clearRoomField;

    private EnemyController _enemyController;
    private NavMeshData _loadedNavMeshData;
    private NavMeshSurface _navMeshSurface;
    
    private int _roomIndex;
    private bool _cleared = false;

    public int RoomIndex => _roomIndex;
    public Room Room { get; private set; }
    
    private void Awake()
    {
        //Component Cache
        _enemyController = GetComponent<EnemyController>();
        _navMeshSurface = GetComponentInChildren<NavMeshSurface>();
        
        var foundGates = GetComponentsInChildren<Gate>().ToList();
        
        foreach (var gate in foundGates)
        {
            gates[gate.roomDirection] = gate;
        }
        
        //모든 문 비활성화
        foreach (var gate in gates.Values)
        {
            gate.gameObject.SetActive(false);
        }
        
        //클리어 필드 비활성화
        SetRoomReady(false);
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
        
        if (Room is { roomType: RoomType.BattleRoom })
        {
            if (_enemyController)
            {
                _enemyController.OnEnemiesClear += RoomCompleted;
            }
        }
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

    public async UniTask OnPlayerEnter(RoomDirection direction = RoomDirection.South)
    {
        if (_navMeshSurface)
        {
            await LoadNavMeshData();
            _navMeshSurface.navMeshData = _loadedNavMeshData;
        }
        
        SetGateOpen(false);
        var gateDirection = (RoomDirection)(((int)direction + 2) % 4);

        var player = GameManager.Instance.Player;

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.TeleportByTransform(player.gameObject, gates[gateDirection].playerSpawnPoint);
        
        SetRoomReady(true);
        gameObject.SetActive(true);
    }

    private async UniTask LoadNavMeshData()
    {
        if(_loadedNavMeshData) return;
        _loadedNavMeshData = await DataManager.Instance.LoadData<NavMeshData>(Addresses.Data.Room.NavMeshData);
    }

    private void ConfigureNavMesh()
    {
        //타깃 설정
        _navMeshSurface.collectObjects = CollectObjects.MarkedWithModifier;
        _navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        
        _navMeshSurface.defaultArea = 0;  // Walkable
        _navMeshSurface.agentTypeID = 0;  // 기본 에이전트

        _navMeshSurface.minRegionArea = 2;
        
        // 레이어 설정
        _navMeshSurface.layerMask = LayerMask.GetMask("Default", "Environment");
        
        // 빌드 시 기존 데이터 정리
        _navMeshSurface.RemoveData();
    }

    public void OnPlayerExit()
    {
        if (!_navMeshSurface)
        {
            Debug.Log("NavMesh Surface is null");
        }
        else
        {
            _navMeshSurface.RemoveData();
        }
        
        SetGateOpen(false);
        gameObject.SetActive(false);
    }

    private void Reward()
    {
        //TODO: 보상 지급
    }

    public void SetRoomReady(bool isEnable)
    {
        if (clearRoomField)
        {
            clearRoomField.gameObject.SetActive(isEnable);
        }
    }

    private void RoomCompleted()
    {
        //해제
        if (_enemyController)
        {
            _enemyController.OnEnemiesClear -= RoomCompleted;
        }
        
        _cleared = true;
        Reward();
        GameManager.Instance.ChangeGameState(GameState.RoomClear);
    }

    public void ClearRoom()
    {
        //적 없애기
        if (_enemyController)
        {
            _enemyController.ClearAllEnemies();
        }
    }

    public void OnNext(bool reached)
    {
        if (reached && !_cleared)
        {
            RoomCompleted();
        }
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }
}
