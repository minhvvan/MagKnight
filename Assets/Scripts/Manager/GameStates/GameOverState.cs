
using hvvan;
using Moon;

public class GameOverState: IGameState
{
    public void OnEnter()
    {
        //TODO: 캐릭터 사망 연출(UI변경 등)
        UIManager.Instance.ShowGameResultUI(GameManager.Instance.CurrentRunData, GameResult.GameOver);
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        //basecamp로 이동
        //SceneController.TransitionToScene(Constants.BaseCamp);
        
    }
}