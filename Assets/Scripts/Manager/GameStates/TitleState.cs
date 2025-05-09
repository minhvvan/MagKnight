
using hvvan;

public class TitleState: IGameState
{
    public async void OnEnter()
    {
        await GameManager.Instance.SetPlayerData(SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData));

        var currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);

        //currentRunData 설정
        GameManager.Instance.SetCurrentRunData(currentRunData); //null이면 생성
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}