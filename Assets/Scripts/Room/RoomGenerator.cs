
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

public enum RoomDirection
{
    East,
    South,
    West,
    North,
}

public enum RoomType
{
    None,
    StartRoom,
    BoosRoom,
    ShopRoom,
    BattleRoom,
    TreasureRoom,
    TrapRoom,
    Max
}

[Serializable]
public class Room
{
    public string roomName;
    public List<int> connectedRooms = new List<int>() { Empty, Empty, Empty, Empty };
    public RoomType roomType;

    public const int Blocked = -10;
    public const int Empty = -1;
    
    public Room()
    {
        roomName = "";
        roomType = RoomType.None;
    }
        
    public Room(Room room)
    {
        roomName = room.roomName;
        roomType = room.roomType;
    }
}

public class RoomGenerator: MonoBehaviour
{
    private List<Room> _rooms = new List<Room>();
    private int _seed;
    private RoomDataSO _roomData;

    private async UniTask Initialize()
    {
        _roomData = await DataManager.Instance.LoadDataAsync<RoomDataSO>(Addresses.Data.Room.RoomData);
    }

    public async void GenerateRooms()
    {
        if (_roomData == null) await Initialize();
        
        //seed 설정
        var currentSaveData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);
        if (currentSaveData == null) 
        {
            Debug.Log("CurrentSaveData is null");
            return;
        }
        
        _seed = currentSaveData.seed;
        Random.InitState(_seed);
        
        ClearRooms();
        
        CreateRooms();

        SetUpDefault();

        ConnectRooms();
        
        DebugPrint();
    }

    private void SetUpDefault()
    {
        var startRoom = _rooms[0];
        startRoom.connectedRooms[(int)RoomDirection.South] = Room.Blocked;
        
        var bossRoom = _rooms[1];
        bossRoom.connectedRooms[(int)RoomDirection.North] = Room.Blocked;
        bossRoom.connectedRooms[(int)RoomDirection.East] = Room.Blocked;
        bossRoom.connectedRooms[(int)RoomDirection.West] = Room.Blocked;
    }

    private void DebugPrint()
    {
        foreach (var room in _rooms)
        {
            string output = "";
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                if(room.connectedRooms[i] == Room.Empty) output += "empty\n";
                else if(room.connectedRooms[i] == Room.Blocked) output += "blocked\n";
                else output += $"{room.connectedRooms[i]}: " + _rooms[room.connectedRooms[i]].roomName + "\n";
            }
            
            Debug.Log($"RoomName: {room.roomName} \n" + output);
        }
    }

    private void CreateRooms()
    {
        var generateRoomCount = Random.Range(Constants.MinRooms, Constants.MaxRooms);
        
        //generateRoomCount만큼 생성 + 시작 + 보스 + 상점
        //필수 지점 추가
        _rooms.Add(new Room()
        {
            roomName = "start",
            roomType = RoomType.StartRoom,
        });
        
        _rooms.Add(new Room()
        {
            roomName = "boss",
            roomType = RoomType.BoosRoom,
        });
        
        _rooms.Add(new Room()
        {
            roomName = "shop",
            roomType = RoomType.ShopRoom,
        });

        //treasureRoom 개수 제한
        int treasureRoomCount = 0;
        for (int i = 0; i < generateRoomCount; i++)
        {
            var type = (RoomType)Random.Range((int)RoomType.BattleRoom, (int)RoomType.Max);
            if (type == RoomType.TreasureRoom)
            {
                treasureRoomCount++;
                if (treasureRoomCount >= Constants.MaxTreasureRoomCount)
                {
                    i--;
                    continue;
                }
            }
            
            var room = _roomData.rooms[type];
            
            _rooms.Add(new Room(room));
        }
    }
    
    private void ConnectRooms()
    {
        //연결 그래프 생성
        HashSet<Room> connectedRooms = new HashSet<Room> { _rooms[0] }; // 시작 방
        HashSet<Room> remainingRooms = new HashSet<Room>();
        
        for (var i = 1; i < _rooms.Count; i++)
        {
            remainingRooms.Add(_rooms[i]);
        }

        // 모든 방이 연결될 때까지 반복
        while (connectedRooms.Count < _rooms.Count)
        {
            // 이미 연결된 방 중 하나를 무작위로 선택
            int connectedRoomIdx = Random.Range(0, connectedRooms.Count);
            var connectedRoom = connectedRooms.ElementAt(connectedRoomIdx);

            // 아직 연결되지 않은 방 중 하나를 무작위로 선택
            int remainingRoomIdx = Random.Range(0, remainingRooms.Count);
            var newRoom = remainingRooms.ElementAt(remainingRoomIdx);
            
            // 두 방을 연결
            if (TryConnect(connectedRoom, newRoom))
            {
                connectedRooms.Add(newRoom);
                remainingRooms.Remove(newRoom);
            }
        }
        
        //추가 연결
        float threshold = Mathf.Log(_rooms.Count) / _rooms.Count;
        for (var i = 0; i < _rooms.Count; i++)
        {
            for (var j = i + 1; j < _rooms.Count; j++)
            {
                //i <-> j 확률로 연결
                var connectProb = Random.value;
                if (connectProb <= threshold)
                {
                    TryConnect(_rooms[i], _rooms[j]);
                }
            }
        }
    }
    
    private bool TryConnect(Room lhs, Room rhs)
    {
        //start <-> boss 연결 방지
        if(lhs.roomType == RoomType.StartRoom && rhs.roomType == RoomType.BoosRoom) return false;
        if(rhs.roomType == RoomType.StartRoom && lhs.roomType == RoomType.BoosRoom) return false;
        
        //중복 연결 방지
        if (lhs.connectedRooms.Contains(_rooms.IndexOf(rhs))) return false;
        
        var unconnectedRoomIndexes = new List<int>();
        for (var i = 0; i < lhs.connectedRooms.Count; i++)
        {
            if (lhs.connectedRooms[i] == Room.Empty)
            {
                unconnectedRoomIndexes.Add(i);
            }
        }

        if (unconnectedRoomIndexes.Count == 0) return false;
        var randomIndex = unconnectedRoomIndexes[Random.Range(0, unconnectedRoomIndexes.Count)];
        var otherDoorIndex = GetOppositeDoor(randomIndex);
        
        //이미 연결된 방향
        if(rhs.connectedRooms[otherDoorIndex] != Room.Empty) return false;

        lhs.connectedRooms[randomIndex] = _rooms.IndexOf(rhs);
        rhs.connectedRooms[otherDoorIndex] = _rooms.IndexOf(lhs);
        
        return true;
    }
    
    private int GetOppositeDoor(int door)
    {
        return (door + 2) % 4;
    }

    private void ClearRooms()
    {
        _rooms.Clear();
    }
}
