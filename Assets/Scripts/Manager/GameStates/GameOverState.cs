
using hvvan;
using Moon;

public class GameOverState: IGameState
{
    public void OnEnter()
    {
        //TODO: 캐릭터 사망 연출(UI변경 등)
        UIManager.Instance.ShowGameOverUI();
        
        //데이터 초기화
        GameManager.Instance.DeleteData(Constants.CurrentRun);
        GameManager.Instance.SaveData(Constants.PlayerData);
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