
using hvvan;
using UnityEngine;

public class DungeonEnterState: IGameState
{
    public async void OnEnter()
    {
        var currentRunData = GameManager.Instance.CurrentRunData;
        currentRunData.isDungeonEnter = true;
        await GameManager.Instance.SaveData(Constants.CurrentRun);
        
        GameManager.Instance.Player.InitializeByCurrentRunData(currentRunData);
        GameManager.Instance.ChangeGameState(GameState.RoomEnter);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}