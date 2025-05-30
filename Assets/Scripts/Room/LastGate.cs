using System;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using UnityEngine;

public class LastGate : MonoBehaviour
{
    [SerializeField] private bool isLastFloor = false;
    
    void OnEnable()
    {
        GetComponent<Collider>().enabled = false;
        _ = UniTask.Delay(3000).ContinueWith(() =>
        {
            GetComponent<Collider>().enabled = true;
        });
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        if (other.TryGetComponent<PlayerController>(out var playerController))
        {
            if (isLastFloor)
            {
                //게임 클리어
                GameManager.Instance.ChangeGameState(GameState.GameClear);
            }
            else
            {
                //다음 층으로 이동
                await GameManager.Instance.MoveToNextFloor();
            }
        }
    }
}
