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
    DungeonEnter,
    RoomEnter,
    RoomClear,
    BossRoom,
    Dialogue,
    GameClear,
    GameOver,
    Pause,
    Max,
}