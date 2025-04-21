public interface IGameState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}

public enum GameState
{
    None,
    Title,
    InitGame,
    BaseCamp,
    RoomEnter,
    RoomClear,
    Dialogue,
    GameClear,
    GameOver,
    Pause,
    Max
}