using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneController: Singleton<RoomSceneController>
{
    private Dictionary<int, RoomController> _loadedRoomControllers = new Dictionary<int, RoomController>();
    private RoomGenerator _roomGenerator = new RoomGenerator();
    private RoomController _currentRoomController;
    
    public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        await SceneManager.LoadSceneAsync(sceneName, mode);
    }

    //Singleton Awake에서 호출
    protected override void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //*TEST: 임시 클리어 판정 KEY = C
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //현재룸 클리어 판정
            if (_currentRoomController == null)
            {
                Debug.Log("Current room is null");
                return;
            }
            
            _currentRoomController.SetGateOpen(true);
        }
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        _loadedRoomControllers.Clear();
        
        // 룸 생성
        await _roomGenerator.GenerateRooms();
        
        // 컨트롤러 캐싱
        var roomController = FindRoomController(scene);
        if (roomController == null) return;

        // 데이터 처리(시작 룸 설정)
        _currentRoomController = roomController;
        var roomData = _roomGenerator.GetRoom(0);
        _currentRoomController.SetRoomData(roomData, 0);
        _currentRoomController.gameObject.SetActive(true);
        
        _loadedRoomControllers.Add(0, roomController);
        
        // 시작 룸에 연결된 룸들 로드
        await LoadConnectedRooms(_currentRoomController.Room.connectedRooms);
    }

    public async UniTask EnterRoom(int currentRoomIndex, RoomDirection direction)
    {
        Time.timeScale = 0f;
        await Moon.ScreenFader.FadeSceneOut().ToUniTask(this);

        var targetRoomIndex = _loadedRoomControllers[currentRoomIndex].Room.connectedRooms[(int)direction];
        if (targetRoomIndex < 0 || targetRoomIndex >= _loadedRoomControllers.Count)
        {
            Debug.Log("TargetRoomIndex is out of range");
            return;
        }
        
        //targetRoom활성화 + currentRoom비활성화
        RoomController targetController = _loadedRoomControllers[targetRoomIndex];
        if (targetController != null)
        {
            targetController.OnPlayerEnter(direction);
            _currentRoomController = targetController;
        }

        RoomController currentController = _loadedRoomControllers[currentRoomIndex];
        if (currentController != null)
        {
            currentController.OnPlayerExit();
        }
        
        var currentRoom = _roomGenerator.GetRoom(currentRoomIndex);
        var targetRoom = _roomGenerator.GetRoom(targetRoomIndex);
        
        // 현재 방에 연결된 방 중 대상 방에 연결되지 않은 방들 언로드
        var unload = currentRoom.connectedRooms.Except(targetRoom.connectedRooms).ToList();
        unload.Remove(targetRoomIndex);
        await UnloadRooms(unload);
    
        // 타깃 방에 연결된 방 중 현재 방에 연결되지 않은 방들 로드
        var load = targetRoom.connectedRooms.Except(currentRoom.connectedRooms).ToList();
        load.Remove(currentRoomIndex);
        await LoadConnectedRooms(load);

        await Moon.ScreenFader.FadeSceneIn().ToUniTask(this);
        Time.timeScale = 1f;
        SceneTransitionEvent.TriggerSceneTransitionComplete(targetRoom.sceneName, true);
    }


    
    private async UniTask LoadConnectedRooms(List<int> roomIndices)
    {
        foreach (var roomIndex in roomIndices)
        {
            var room = _roomGenerator.GetRoom(roomIndex);
            if (room == null) continue;
            
            // 이미 로드된 씬이면 스킵
            if (_loadedRoomControllers.ContainsKey(roomIndex)) continue;
            
            await LoadAndSetupRoom(room, roomIndex);
        }
    }
    
    private async UniTask LoadAndSetupRoom(Room room, int roomIndex)
    {
        var operation = SceneManager.LoadSceneAsync(room.sceneName, LoadSceneMode.Additive);
        await operation.ToUniTask();
    
        if (operation.isDone)
        {
            // 최근 로드된 씬(순서 보장)
            Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            
            var loadedSceneController = FindRoomController(loadedScene);
            if (loadedSceneController != null)
            {
                _loadedRoomControllers.Add(roomIndex, loadedSceneController);
                loadedSceneController.SetRoomData(room, roomIndex);
                loadedSceneController.gameObject.SetActive(false);
            }
        }
    }
    
    private async UniTask UnloadRooms(List<int> roomIndices)
    {
        foreach (var roomIndex in roomIndices)
        {
            //시작씬 제외
            if(roomIndex == 0) continue;
            
            // _loadedScenes에서 직접 Scene 참조를 가져와 언로드
            if (_loadedRoomControllers.TryGetValue(roomIndex, out var roomController))
            {
                await SceneManager.UnloadSceneAsync(roomController.gameObject.scene).ToUniTask();
                _loadedRoomControllers.Remove(roomIndex);
            }
        }
    }
    
    private RoomController FindRoomController(Scene scene)
    {
        if (!scene.isLoaded)
            return null;
        
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            RoomController controller = root.GetComponent<RoomController>();
            if (controller != null)
                return controller;
        }
    
        return null;
    }
}
