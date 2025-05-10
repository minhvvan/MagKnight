
using hvvan;

public class GameClearState: IGameState
{
    public void OnEnter()
    {
        UIManager.Instance.ShowGameResultUI(GameManager.Instance.CurrentRunData, GameResult.GameOver);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}