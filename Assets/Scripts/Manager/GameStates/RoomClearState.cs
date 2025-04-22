
using hvvan;
using UnityEngine;
using VFolders.Libs;

public class RoomClearState: IGameState
{
    public void OnEnter()
    {
        //CurrentData 최신화
        var currentRunData = GameManager.Instance.CurrentRunData;
        currentRunData.currentRoomIndex = RoomSceneController.Instance.CurrentRoomController.RoomIndex;
        currentRunData.lastPlayerPosition = GameManager.Instance.Player.transform.position;
        currentRunData.lastPlayerRotation = GameManager.Instance.Player.transform.rotation;
        GameManager.Instance.SetCurrentRunData(currentRunData);
        
        //Room 관리
        RoomSceneController.Instance.CurrentRoomController.ClearRoom();
        
        //TODO: 최종 보스 잡았는지 확인
        //잡았으면 GameClear
    }

    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameManager.Instance.ChangeGameState(GameState.GameOver);
        }
    }

    public void OnExit()
    {
    }
}