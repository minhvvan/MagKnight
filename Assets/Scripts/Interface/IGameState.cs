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
    Run,
    Combat,
    Dialogue,
    GameClear,
    Pause,
    GameOver,
    Max
}