
using hvvan;

public class BaseCampState: IGameState
{
    public async void OnEnter()
    {
        //currentRunData가 없으면 생성 및 저장
        if (GameManager.Instance.CurrentRunData == null)
        {
            GameManager.Instance.SetCurrentRunData();
            await GameManager.Instance.SaveData(Constants.CurrentRun);
        }
        
        //스탯 초기화
        GameManager.Instance.Player.InitializeByCurrentRunData(GameManager.Instance.CurrentRunData);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}