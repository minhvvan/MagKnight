
using hvvan;
using UnityEngine;
using VFolders.Libs;

public class RoomClearState: IGameState
{
    public void OnEnter()
    {
        //CurrentData 최신화
        var currentRunData = GameManager.Instance.CurrentRunData;
        currentRunData.currentRoom = RoomSceneController.Instance.CurrentRoomController.Room;
        GameManager.Instance.SetCurrentRunData(currentRunData);
        
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