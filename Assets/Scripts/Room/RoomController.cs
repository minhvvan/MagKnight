using System;
using System.Collections;
using System.Linq;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using hvvan;
using Managers;
using Moon;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class RoomController : MonoBehaviour, IObserver<bool>
{
    [SerializeField] private SerializedDictionary<RoomDirection, Gate> gates;
    [SerializeField] private LastGate nextFloorGate;
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

        if (nextFloorGate)
        {
            nextFloorGate.gameObject.SetActive(false);
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
        
        if (Room is { roomType: RoomType.BattleRoom } or { roomType: RoomType.BoosRoom })
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
        
        //마지막 문 제어
        if (nextFloorGate)
        {
            nextFloorGate.gameObject.SetActive(isOpen);
        }
    }

    public async UniTask OnPlayerEnter(RoomDirection direction = RoomDirection.South, bool moveForce = false)
    {
        if (_navMeshSurface)
        {
            await LoadNavMeshData();
            _navMeshSurface.navMeshData = _loadedNavMeshData;
        }
        
        gameObject.SetActive(true);

        SetGateOpen(false);
        var gateDirection = (RoomDirection)(((int)direction + 2) % 4);

        var player = GameManager.Instance.Player;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller && gates.ContainsKey(gateDirection))
        {
            controller.TeleportByTransform(player.gameObject, gates[gateDirection].playerSpawnPoint);
        }
        player.AbilitySystem.ClearTag();
        
        //함정방 입장 연출
        if (Room.roomType is RoomType.TrapRoom)
        {
            if( !GameManager.Instance.CurrentRunData.clearedRooms.Contains(_roomIndex))
            {
                var camera = GameObject.Find("CameraRig");
                if (camera && camera.TryGetComponent<CameraSettings>(out var cameraSettings))
                {
                    StartCoroutine(LookClearField(cameraSettings));
                }
            }
        }
        else
        {
            // 앞쪽으로 조금 이동(연출)
            if (moveForce)
            {
                if (player.TryGetComponent<PlayerController>(out var playerController))
                {
                    var target = new GameObject()
                    {
                        transform = { position = player.transform.position + gates[gateDirection].playerSpawnPoint.forward * 5 }
                    };
            
                    playerController.MoveForce(target.transform);
                }
            }
        }
        
        //gate 표시 연결
        BindGateIndicator();
        
        SetRoomReady(true);
        if (Room.roomType is RoomType.BattleRoom or RoomType.BoosRoom && !GameManager.Instance.CurrentRunData.clearedRooms.Contains(_roomIndex))
        {
            cancelTokenSource = new CancellationTokenSource();
            ChargeSkillGauge(cancelTokenSource.Token).Forget();
        }

        if (Room.roomType is RoomType.BoosRoom)
        {
            GameManager.Instance.ChangeGameState(GameState.BossRoom);
        }
    }

    private IEnumerator LookClearField(CameraSettings cameraSettings)
    {
        GameManager.Instance.Player.InputHandler.ReleaseControl();
        
        yield return new WaitForSeconds(1f);
        
        var origin = cameraSettings.Current.LookAt;
        
        // 임시 Transform 생성
        GameObject tempObj = new GameObject("TempLookAt");
        tempObj.transform.position = origin.position;
        cameraSettings.Current.LookAt = tempObj.transform;
        
        // Sequence 생성
        Sequence sequence = DOTween.Sequence();
        
        // 앞으로 이동
        sequence.Append(tempObj.transform.DOMove(clearRoomField.transform.position, 1.5f)
            .SetEase(Ease.InOutSine));
        
        // 일정 시간 대기
        sequence.AppendInterval(.5f);
        
        // 원래 위치로 돌아가기
        sequence.Append(tempObj.transform.DOMove(origin.position, 1.5f)
            .SetEase(Ease.InOutSine));
        
        // 완료 후 콜백
        sequence.OnComplete(() => {
            cameraSettings.Current.LookAt = origin;
            Destroy(tempObj);
            GameManager.Instance.Player.InputHandler.GainControl();
        });
        
        // 코루틴에서는 시퀀스가 완료될 때까지 대기
        yield return sequence.WaitForCompletion();
    }
    
    private async UniTask LoadNavMeshData()
    {
        if(_loadedNavMeshData) return;
        var address = Addresses.NavMeshAddressLoader.GetPath(Room.roomType);
        _loadedNavMeshData = await DataManager.Instance.LoadData<NavMeshData>(address);
    }

    public void OnPlayerExit()
    {
        if (_navMeshSurface)
        {
            _navMeshSurface.RemoveData();
        }
        
        UnbindGateIndicator();

        SetGateOpen(false);
        gameObject.SetActive(false);
    }

    private void Reward()
    {
        //TODO: 파츠도 생성되도록 랜덤 추가
        var rotation = Quaternion.Euler(0, 180, 0);
        ItemManager.Instance.SpawnLootCrate(ItemCategory.Artifact, ItemRarity.Common, new Vector3(0,1f,0), rotation);
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
        
        
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Common.Light);
        
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

    private void OnDestroy()
    {
        if (RoomSceneController.Instance.CurrentRoomController != this) return;
        var gateIndicator = UIManager.Instance?.inGameUIController?.gateIndicatorUIController;
        if (gateIndicator)
        {
            gateIndicator.ClearGateBind();
        }
    }

    private void BindGateIndicator()
    {
        var gateIndicator = UIManager.Instance.inGameUIController.gateIndicatorUIController;
        for (var i = 0; i < Room.connectedRooms.Count; i++)
        {
            if(Room.connectedRooms[i] == Room.Empty || Room.connectedRooms[i] == Room.Blocked) continue;
            gateIndicator.BindGate(gates[(RoomDirection)i]);
        }
    }
    
    private void UnbindGateIndicator()
    {
        var gateIndicator = UIManager.Instance.inGameUIController.gateIndicatorUIController;
        for (var i = 0; i < Room.connectedRooms.Count; i++)
        {
            if(Room.connectedRooms[i] == Room.Empty || Room.connectedRooms[i] == Room.Blocked) continue;
            gateIndicator.UnBindGate(gates[(RoomDirection)i]);
        }
    }
}
