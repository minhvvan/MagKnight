
using Cysharp.Threading.Tasks;
using hvvan;
using UnityEngine;

public class BaseCampState: IGameState
{
    public async void OnEnter()
    {
        GameManager.Instance.SetCurrentRunData();
        GameManager.Instance.Player.InitStat(GameManager.Instance.GetCurrentStat());
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}