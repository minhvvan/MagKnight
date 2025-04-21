using hvvan;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moon
{
    public class BaseCampGate : MonoBehaviour
    {
        [SerializeField] string sceneName = "Prototype"; 

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SceneController.TransitionToScene(sceneName, SceneLoaded);
            }
        }

        private void SceneLoaded()
        {
            RoomSceneController.Instance.EnterFloor();
            GameManager.Instance.ChangeGameState(GameState.RoomEnter);
        }
    }
}