
using hvvan;
using UnityEngine;

public class DungeonEnterState: IGameState
{
    public async void OnEnter()
    {
        //현재 데이터로 spawn된 player stat초기화
        var currentRunData = GameManager.Instance.CurrentRunData;
        GameManager.Instance.Player.InitializeByCurrentRunData(currentRunData);
        
        //던전 입장 세팅
        currentRunData.isDungeonEnter = true;
        await GameManager.Instance.SaveData(Constants.CurrentRun);
        
        GameManager.Instance.ChangeGameState(GameState.RoomEnter);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}