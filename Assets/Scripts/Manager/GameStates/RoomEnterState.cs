
using System;
using hvvan;
using UnityEngine;

public class RoomEnterState: IGameState
{
    private RoomController _currentRoomController;
    
    public void OnEnter()
    {
        _currentRoomController = RoomSceneController.Instance.CurrentRoomController;
        if (_currentRoomController)
        {
            if (GameManager.Instance.CurrentRunData.clearedRooms.Contains(_currentRoomController.RoomIndex))
            {
                GameManager.Instance.ChangeGameState(GameState.RoomClear);
            }
        }
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        _currentRoomController = null;
    }
}