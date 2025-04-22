using DG.Tweening;
using hvvan;
using UnityEngine;
using UnityEngine.UI;

namespace Moon
{
    public class IntroUIController : MonoBehaviour
    {
        void Update()
        {
            if (Input.anyKeyDown)
            {
                DOTween.KillAll();
                GameManager.Instance.ChangeGameState(GameState.InitGame);
            }
        }
    }
}