
using hvvan;
using UnityEngine;

public class RoomClearState: IGameState
{
    public void OnEnter()
    {
        //CurrentData 최신화
        var currentRunData = GameManager.Instance.CurrentRunData;
        currentRunData.currentRoomIndex = RoomSceneController.Instance.CurrentRoomController.RoomIndex;
        currentRunData.lastPlayerPosition = GameManager.Instance.Player.transform.position;
        currentRunData.lastPlayerRotation = GameManager.Instance.Player.transform.rotation;
        currentRunData.clearedRooms.Add(RoomSceneController.Instance.CurrentRoomController.RoomIndex);
        GameManager.Instance.SetCurrentRunData(currentRunData);
        GameManager.Instance.SaveData(Constants.CurrentRun);
        
        //Room 관리
        RoomSceneController.Instance.CurrentRoomController.SetGateOpen(true);
        
        //TODO: 최종 보스 잡았는지 확인
        //잡았으면 GameClear
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}