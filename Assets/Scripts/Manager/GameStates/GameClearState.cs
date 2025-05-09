
using hvvan;

public class GameClearState: IGameState
{
    public async void OnEnter()
    {
        //TODO: 클리어 연출(엔딩 크래딧?)
        
        //데이터 관리(current삭제 및 재화 저장)
        GameManager.Instance.DeleteData(Constants.CurrentRun);
        await GameManager.Instance.SaveData(Constants.PlayerData);
        
        //베이스 캠프로 이동
        GameManager.Instance.ChangeGameState(GameState.InitGame);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}