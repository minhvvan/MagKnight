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
    Loading,
    BaseCamp,
    Dungeon,
    Combat,
    Dialogue,
    Victory,
    Pause,
    Tutorial,
    GameOver,
    Max
}