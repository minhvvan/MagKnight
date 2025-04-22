
using hvvan;
using Moon;
using UnityEngine;

public class InitGameState: IGameState
{
    public async void OnEnter()
    {
        await GameManager.Instance.SetPlayerData(SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData));

        var currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);

        //TODO: UI랑 동기화 필요, 현재는 데이터를 로드하고 나서 로딩 화면을 보여줌 => 로딩화면 보여주면서 데이터 로드하도록 변경 필요
        if (currentRunData == null)
        {
            //회차 정보 없음 => BaseCamp로
            SceneController.TransitionToScene(Constants.BaseCamp);
            GameManager.Instance.ChangeGameState(GameState.BaseCamp);
        }
        else
        {
            //회차 정보대로 씬 이동 및 설정
            GameManager.Instance.SetCurrentRunData(currentRunData);

            //TODO: startScene으로 보내고 이동시켜야 함
            SceneController.TransitionToScene(currentRunData.currentRoom.sceneName);
            GameManager.Instance.ChangeGameState(GameState.RoomClear);
        }
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}