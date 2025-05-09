
using hvvan;

public class GameClearState: IGameState
{
    public async void OnEnter()
    {
        //TODO: UI로 아래 로직을 컨트롤해야 함
        //UI에서 확인 버튼이 눌리면 아래가 실행되도록
        
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