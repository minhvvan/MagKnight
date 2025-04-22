
using hvvan;
using UnityEngine;

public class PauseState: IGameState
{
    public void OnEnter()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowPauseMenuUI();
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HidePauseMenuUI();
    }
}