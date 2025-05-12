using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using hvvan;
using UnityEngine;

namespace Moon
{
    public class BaseCampGate : MonoBehaviour
    {
        [SerializeField] string sceneName = "Prototype"; 
        [SerializeField] private bool checkWeapon = false;
        public Action<PlayerController> OnEnterWithoutWeapon;
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ValidateTransitions(other);
            }
        }

        private void ValidateTransitions(Collider other)
        {
            if (!other.TryGetComponent<PlayerController>(out var player)) return;
            if (!checkWeapon)
            {
                SceneController.TransitionToScene(sceneName, true, SceneLoaded);
                return;
            }
            
            if (player.WeaponHandler.CurrentWeaponType == WeaponType.None)
            {
                OnEnterWithoutWeapon?.Invoke(player);
            }
            else
            {
                SceneController.TransitionToScene(sceneName, true, SceneLoaded);
            }
        }

        private IEnumerator SceneLoaded()
        {
            //트레이닝 룸 예외 처리
            if (!checkWeapon) 
            {
                var currentRunData = GameManager.Instance.CurrentRunData;
                GameManager.Instance.Player.InitializeByCurrentRunData(currentRunData);
                yield break;
            }
            
            GameManager.Instance.ChangeGameState(GameState.DungeonEnter);
            
            var enterTask = RoomSceneController.Instance.EnterFloor();
            while (!enterTask.Status.IsCompleted())
            {
                yield return null;
            }
            
            var enterRoomTask = RoomSceneController.Instance.CurrentRoomController.OnPlayerEnter(RoomDirection.North, true);
            while (!enterRoomTask.Status.IsCompleted())
            {
                yield return null;
            }
            
            RoomSceneController.Instance.CurrentRoomController.SetRoomReady(true);
        }
    }
}