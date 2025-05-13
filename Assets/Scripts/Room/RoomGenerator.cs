
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

public enum RoomDirection
{
    East,
    South,
    West,
    North,
    Max
}

public enum RoomType
{
    None,
    StartRoom,
    BossRoom,
    ShopRoom,
    BattleRoom,
    TreasureRoom,
    TrapRoom,
    Max
}

[Serializable]
public class Room
{
    public string roomTitle;
    public string sceneName;
    public List<int> connectedRooms = new List<int>() { Empty, Empty, Empty, Empty };
    public RoomType roomType;

    public const int Blocked = -10;
    public const int Empty = -1;
    
    public Room()
    {
        sceneName = "";
        roomType = RoomType.None;
    }
        
    public Room(Room room)
    {
        roomTitle = room.roomTitle;
        sceneName = room.sceneName;
        roomType = room.roomType;
    }
}

public class RoomGenerator
{
    private List<Room> _rooms = new List<Room>();
    private int _seed;
    private FloorDataSO _floorData;

    private async UniTask Initialize()
    {
        _floorData = await DataManager.Instance.LoadScriptableObjectAsync<FloorDataSO>(Addresses.Data.Room.Floor);
    }

    public async UniTask GenerateRooms()
    {
        if (_floorData == null) await Initialize();
        
        //seed 설정
        var currentSaveData = GameManager.Instance.CurrentRunData;

        _seed = currentSaveData.seed;
        Random.InitState(_seed);
        
        ClearRooms();
        
        CreateRooms();

        SetUpDefault();

        ConnectRooms();
        
        // DebugPrint();
    }

    private void SetUpDefault()
    {
        //시작 남쪽 막기
        var startRoom = _rooms[0];
        startRoom.connectedRooms[(int)RoomDirection.South] = Room.Blocked;
        
        //보스방 남쪽 제외 모든 방향 막기
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
                else output += $"{room.connectedRooms[i]}: " + _rooms[room.connectedRooms[i]].sceneName + "\n";
            }
            
            Debug.Log($"RoomName: {room.sceneName} \n" + output);
        }
    }

    private void CreateRooms()
    {
        var currentFloor = GameManager.Instance.CurrentRunData.currentFloor;

        //room 개수 제한
        Dictionary<RoomType, int> generatedRoomCount = new();

        var seed = GameManager.Instance.CurrentRunData.seed;
        
        for (int roomType = (int)RoomType.StartRoom; roomType < (int)RoomType.Max; roomType++)
        {
            //min 개수만큼 생성
            generatedRoomCount.Add((RoomType)roomType, 0);

            int currentSeed = seed + (roomType * 1000);

            var genNum = Random.Range(_floorData.Floor[currentFloor].roomNumLimit[(RoomType)roomType].min, _floorData.Floor[currentFloor].roomNumLimit[(RoomType)roomType].max);
    
            for (int j = 0; j < genNum; j++)
            {
                // 각 반복마다 시드 변경 (j에 따라)
                currentSeed += (j * 137); // 137은 임의의 소수로 분포를 좋게 함
                Random.InitState(currentSeed);
        
                var roomList = _floorData.Floor[currentFloor].rooms[(RoomType)roomType];
                var room = roomList[Random.Range(0, roomList.Count)];
        
                _rooms.Add(new Room(room));
                generatedRoomCount[(RoomType)roomType]++;
            }
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
        if(lhs.roomType == RoomType.StartRoom && rhs.roomType == RoomType.BossRoom) return false;
        if(rhs.roomType == RoomType.StartRoom && lhs.roomType == RoomType.BossRoom) return false;
        
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

    public Room GetRoom(int roomIndex)
    {
        return roomIndex < _rooms.Count && roomIndex >= 0 ? _rooms[roomIndex] : null;
    }
}
