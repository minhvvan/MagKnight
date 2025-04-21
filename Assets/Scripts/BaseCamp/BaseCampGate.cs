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
                SceneController.TransitionToScene(sceneName, SceneLoaded);
            }
        }

        private IEnumerator SceneLoaded()
        {
            yield return RoomSceneController.Instance.EnterFloor();
            
            //TODO: PlayerCurrentStat 초기화
            GameManager.Instance.ChangeGameState(GameState.RoomEnter);
        }
    }
}