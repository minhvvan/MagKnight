
using System.Collections;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Moon;
using UnityEngine;

public class InitGameState: IGameState
{
    public async void OnEnter()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        if (currentRunData == null)
        {
            await GameManager.Instance.SetPlayerData(SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData));
            currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);
            //currentRunData 설정
            GameManager.Instance.SetCurrentRunData(currentRunData); //null이면 생성
            currentRunData = GameManager.Instance.CurrentRunData;
        }
        
        await ItemManager.Instance.SetAllItemUpdate(currentRunData);
        
        var playerData = await GameManager.Instance.GetPlayerData();
        UIManager.Instance.inGameUIController.currencyUIController.InitializeCurrencyUI(playerData);
        
        if (!currentRunData.isDungeonEnter)
        {
            //던전 입장 X => BaseCamp로
            SceneController.TransitionToScene(Constants.BaseCamp, true, TransitionToBaseCampCallback);
        }
        else
        {
            //회차 정보대로 씬 이동 및 설정

            var floorList = await DataManager.Instance.LoadScriptableObjectAsync<FloorDataSO>(Addresses.Data.Room.Floor);
            var currentFloorRooms = floorList.Floor[currentRunData.currentFloor];
            
            //시작씬으로 이동 -> 시작씬 로드 이후 최근 저장 위치로 이동
            SceneController.TransitionToScene(currentFloorRooms.rooms[RoomType.StartRoom][0].sceneName, false, MoveToLastRoom);
        }
        
        //보스체력바 강제 비활성화
        UIManager.Instance.inGameUIController.ImmediateHideInGameUI();
    }

    private IEnumerator TransitionToBaseCampCallback()
    {
        GameManager.Instance.ChangeGameState(GameState.BaseCamp);
        yield break;
    }

    private IEnumerator MoveToLastRoom()
    {
        var enterFloorTask = RoomSceneController.Instance.EnterFloor(false, false);
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
            player.InitializeByCurrentRunData(GameManager.Instance.CurrentRunData);
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
    }
}