using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Moon
{
    public class IntroUIController : MonoBehaviour
    {
        const string k_NextSceneName = "prototype";


        void Update()
        {
            if (Input.anyKeyDown)
            {
                DOTween.KillAll();
                SceneController.TransitionToScene(k_NextSceneName);
            }
        }
    }
}