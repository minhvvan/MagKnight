using UnityEngine;

namespace Moon
{
    public class BaseCampGate : MonoBehaviour
    {
        [SerializeField] string sceneName = "Prototype"; 

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SceneController.TransitionToScene(sceneName);
            }
        }

    }
}