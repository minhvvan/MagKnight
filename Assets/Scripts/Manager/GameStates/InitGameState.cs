
using System.Collections;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Moon;

public class InitGameState: IGameState
{
    private CurrentRunData _currentRunData;
    
    public async void OnEnter()
    {
        await GameManager.Instance.SetPlayerData(SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData));

        _currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);

        //TODO: UI랑 동기화 필요, 현재는 데이터를 로드하고 나서 로딩 화면을 보여줌 => 로딩화면 보여주면서 데이터 로드하도록 변경 필요
        if (_currentRunData == null)
        {
            //회차 정보 없음 => BaseCamp로
            SceneController.TransitionToScene(Constants.BaseCamp);
            GameManager.Instance.ChangeGameState(GameState.BaseCamp);
        }
        else
        {
            //회차 정보대로 씬 이동 및 설정
            GameManager.Instance.SetCurrentRunData(_currentRunData);

            var floorList = await DataManager.Instance.LoadDataAsync<FloorDataSO>(Addresses.Data.Room.Floor);
            var currentFloorRooms = floorList.Floor[_currentRunData.currentFloor];
            
            //시작씬으로 이동
            SceneController.TransitionToScene(currentFloorRooms.rooms[RoomType.StartRoom].sceneName, MoveToLastRoom);
        }
    }

    private IEnumerator MoveToLastRoom()
    {
        var enterFloorTask = RoomSceneController.Instance.EnterFloor();
        while (!enterFloorTask.Status.IsCompleted())
        {
            yield return null;
        }
    
        var teleportTask = RoomSceneController.Instance.TeleportToSavedRoom();
        while (!teleportTask.Status.IsCompleted())
        {
            yield return null;
        }
        
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