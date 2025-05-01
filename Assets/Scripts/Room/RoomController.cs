using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    [SerializeField] private bool hasReward;

    private EnemyController _enemyController;
    private NavMeshData _loadedNavMeshData;
    private NavMeshSurface _navMeshSurface;
    
    private int _roomIndex;
    private bool _cleared = false;
    private CancellationTokenSource cancelTokenSource;
    
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
        if (Room.roomType is RoomType.BattleRoom or RoomType.BoosRoom && !GameManager.Instance.CurrentRunData.clearedRooms.Contains(_roomIndex))
        {
            cancelTokenSource = new CancellationTokenSource();
            ChargeSkillGauge(cancelTokenSource.Token).Forget();
        }
    }

    private async UniTask LoadNavMeshData()
    {
        if(_loadedNavMeshData) return;
        _loadedNavMeshData = await DataManager.Instance.LoadData<NavMeshData>(Addresses.Data.Room.NavMeshData);
    }

    public void OnPlayerExit()
    {
        if (_navMeshSurface)
        {
            _navMeshSurface.RemoveData();
        }
        
        SetGateOpen(false);
        gameObject.SetActive(false);
    }

    private void Reward()
    {
        ItemManager.Instance.SpawnLootCrate(ItemCategory.Artifact, ItemRarity.Common, new Vector3(0,1f,0), Quaternion.identity);
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
        if(hasReward) Reward();
        
        ClearRoom();
    }

    public void ClearRoom()
    {
        //적 없애기
        if (_enemyController)
        {
            _enemyController.ClearAllEnemies();
        }
        
        //클리어 필드 없애기
        if (clearRoomField)
        {
            clearRoomField.gameObject.SetActive(false);
        }
        
        //스킬게이지 중지
        cancelTokenSource?.Cancel();
        cancelTokenSource?.Dispose();
        cancelTokenSource = null;
        
        SetGateOpen(true);
        GameManager.Instance.ChangeGameState(GameState.RoomClear);
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

    private async UniTaskVoid ChargeSkillGauge(CancellationToken token)
    {
        var playerASC = GameManager.Instance.Player.AbilitySystem;
        var effect = new GameplayEffect(EffectType.Instant, AttributeType.SkillGauge, 1);

        try
        {
            while(true)
            {
                token.ThrowIfCancellationRequested();
                playerASC.ApplyEffect(effect);
                
                await UniTask.Delay(2000, cancellationToken: token);
            }
        }
        catch (OperationCanceledException) {}
    }
}
