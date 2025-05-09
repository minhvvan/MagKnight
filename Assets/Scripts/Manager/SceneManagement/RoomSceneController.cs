using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using hvvan;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneController: Singleton<RoomSceneController>
{
    private Dictionary<int, RoomController> _loadedRoomControllers = new Dictionary<int, RoomController>();
    private RoomGenerator _roomGenerator = new RoomGenerator();
    private RoomController _currentRoomController;
    public RoomController CurrentRoomController => _currentRoomController;
    
    public async UniTask EnterFloor(bool loadConnect = true)
    {
        _loadedRoomControllers.Clear();
        
        // 룸 생성
        await _roomGenerator.GenerateRooms();
        
        // 데이터 처리(시작 룸 설정)
        _currentRoomController = FindObjectOfType<RoomController>();
        var roomData = _roomGenerator.GetRoom(0);
        _currentRoomController.SetRoomData(roomData, 0);
        _currentRoomController.gameObject.SetActive(true);
        
        _loadedRoomControllers.Add(0, _currentRoomController);
        
        // 시작 룸에 연결된 룸들 로드
        if (loadConnect)
        {
            await LoadConnectedRooms(_currentRoomController.Room.connectedRooms);
        }
    }

    public async UniTask EnterRoom(int currentRoomIndex, RoomDirection direction)
    {
        Time.timeScale = 0f;
        await Moon.ScreenFader.FadeSceneOut().ToUniTask(this);

        var targetRoomIndex = _loadedRoomControllers[currentRoomIndex].Room.connectedRooms[(int)direction];
        if (targetRoomIndex < 0 || !_loadedRoomControllers.TryGetValue(targetRoomIndex, out var targetController))
        {
            Debug.Log("TargetRoomIndex is out of range");
            return;
        }
        
        //targetRoom활성화 + currentRoom비활성화
        if (targetController != null)
        {
            _currentRoomController = targetController;
            GameManager.Instance.ChangeGameState(GameState.RoomEnter);
            
            await targetController.OnPlayerEnter(direction, true);

            if (GameManager.Instance.CurrentRunData.clearedRooms.Contains(targetRoomIndex))
            {
                targetController.ClearRoom();
            }
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
        SceneTransitionEvent.TriggerSceneTransitionComplete(targetRoom.roomTitle, true);
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
                _loadedRoomControllers.TryAdd(roomIndex, loadedSceneController);
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
            if (controller)
                return controller;
        }
    
        return null;
    }

    public async UniTask TeleportToSavedRoom()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        if (currentRunData.currentRoomIndex != 0)
        {
            var targetRoom = _roomGenerator.GetRoom(currentRunData.currentRoomIndex);
            var operation = SceneManager.LoadSceneAsync(targetRoom.sceneName, LoadSceneMode.Additive);
            await operation.ToUniTask();

            if (operation.isDone)
            {
                Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                //현재 roomController 캐싱
                var loadedSceneController = FindRoomController(loadedScene);
                if (loadedSceneController)
                {
                    //Room 데이터 초기화
                    loadedSceneController.SetRoomData(targetRoom, currentRunData.currentRoomIndex);
                    _currentRoomController = loadedSceneController;
                    
                    _loadedRoomControllers.TryAdd(currentRunData.currentRoomIndex, loadedSceneController);
                    await loadedSceneController.OnPlayerEnter();
                }
            }
            
            //start룸 비활성화
            _loadedRoomControllers[0].OnPlayerExit();
            
            //최종 클리어 방 타이틀 출력
            SceneTransitionEvent.TriggerSceneTransitionComplete(targetRoom.roomTitle, true);
        }
        
        //연결된 룸 load 시도
        await LoadConnectedRooms(_roomGenerator.GetRoom(currentRunData.currentRoomIndex).connectedRooms);

        //Character위치 조정
        var player = GameManager.Instance.Player;
        if (player && player.TryGetComponent<CharacterController>(out var characterController))
        {
            characterController.Teleport(player.gameObject, currentRunData.lastPlayerPosition, currentRunData.lastPlayerRotation);
        }
        else
        {
            Debug.Log("No character controller found");
        }
    }
}
