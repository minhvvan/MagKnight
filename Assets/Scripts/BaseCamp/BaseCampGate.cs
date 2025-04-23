using System.Collections;
using hvvan;
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
                SceneController.TransitionToScene(sceneName, true, SceneLoaded);
            }
        }

        private IEnumerator SceneLoaded()
        {
            GameManager.Instance.ChangeGameState(GameState.DungeonEnter);
            yield return RoomSceneController.Instance.EnterFloor();
        }
    }
}