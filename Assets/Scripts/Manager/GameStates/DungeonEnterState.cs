
using hvvan;

public class DungeonEnterState: IGameState
{
    public void OnEnter()
    {
        //currentRun Data 초기화
        GameManager.Instance.SetCurrentRunData();
        GameManager.Instance.Player.InitStat(GameManager.Instance.GetCurrentStat());
        GameManager.Instance.ChangeGameState(GameState.RoomEnter);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}