
using hvvan;

public class DialogueState: IGameState
{
    private GameState _previousState;
    public void OnEnter()
    {
        _previousState = GameManager.Instance.CurrentGameState;
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        GameManager.Instance.ChangeGameState(_previousState);
    }
}