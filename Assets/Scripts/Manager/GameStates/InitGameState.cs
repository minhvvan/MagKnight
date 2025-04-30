
using System.Collections;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Moon;
using UnityEngine;

public class InitGameState: IGameState
{
    private CurrentRunData _currentRunData;
    
    public async void OnEnter()
    {
        await GameManager.Instance.SetPlayerData(SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData));

        _currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);

        if (_currentRunData == null)
        {
            //currentRunData 생성
            GameManager.Instance.SetCurrentRunData();
            _currentRunData = GameManager.Instance.CurrentRunData;
        }

        if (!_currentRunData.isDungeonEnter)
        {
            //회차 정보 없음 => BaseCamp로
            SceneController.TransitionToScene(Constants.BaseCamp, true, TransitionToBaseCampCallback);
        }
        else
        {
            //회차 정보대로 씬 이동 및 설정
            GameManager.Instance.SetCurrentRunData(_currentRunData);

            var floorList = await DataManager.Instance.LoadScriptableObjectAsync<FloorDataSO>(Addresses.Data.Room.Floor);
            var currentFloorRooms = floorList.Floor[_currentRunData.currentFloor];
            
            //시작씬으로 이동
            SceneController.TransitionToScene(currentFloorRooms.rooms[RoomType.StartRoom].sceneName, false, MoveToLastRoom);
        }
    }

    private IEnumerator TransitionToBaseCampCallback()
    {
        GameManager.Instance.ChangeGameState(GameState.BaseCamp);
        yield break;
    }

    private IEnumerator MoveToLastRoom()
    {
        var enterFloorTask = RoomSceneController.Instance.EnterFloor(false);
        while (!enterFloorTask.Status.IsCompleted())
        {
            yield return null;
        }
    
        var teleportTask = RoomSceneController.Instance.TeleportToSavedRoom();
        while (!teleportTask.Status.IsCompleted())
        {
            yield return null;
        }
        
        //플레이어 설정
        var player = GameManager.Instance.Player;
        if (player)
        {
            player.InitializeByCurrentRunData(_currentRunData);
        }
        
        //룸 클리어
        RoomSceneController.Instance.CurrentRoomController.ClearRoom();
        
        GameManager.Instance.ChangeGameState(GameState.RoomClear);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        _currentRunData = null;
    }
}