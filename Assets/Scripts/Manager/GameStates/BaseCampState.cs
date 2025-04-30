
using Cysharp.Threading.Tasks;
using hvvan;
using UnityEngine;

public class BaseCampState: IGameState
{
    public void OnEnter()
    {
        //currentRunData 생성 및 저장
        GameManager.Instance.SetCurrentRunData();
        
        //스탯 초기화
        GameManager.Instance.Player.InitStat(GameManager.Instance.GetCurrentStat());
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}