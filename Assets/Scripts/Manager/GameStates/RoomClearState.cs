
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
        if(currentRunData.clearedRooms.AddUnique(RoomSceneController.Instance.CurrentRoomController.RoomIndex))
            currentRunData.clearedRoomsCount++;
        
        GameManager.Instance.SetCurrentRunData(currentRunData);
        _ = GameManager.Instance.SaveData(Constants.CurrentRun);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}