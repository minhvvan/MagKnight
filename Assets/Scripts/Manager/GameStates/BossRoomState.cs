using hvvan;

public class BossRoomState : IGameState
{
    public void OnEnter()
    {
        // 추가 로직이 아예 필요 없으면 빈 상태로 두고
        // BGM 전환은 GameManager.ChangeGameState() 안의 switch-case에서 처리.
    }

    public void OnUpdate() { }   // 보스방 고정 상태라면 비워도 무방
    public void OnExit()   { }
}