using System.Collections;
using Cysharp.Threading.Tasks;
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
            
            var enterTask = RoomSceneController.Instance.EnterFloor();
            while (!enterTask.Status.IsCompleted())
            {
                yield return null;
            }
            
            RoomSceneController.Instance.CurrentRoomController.SetClearField(true);
        }
    }
}