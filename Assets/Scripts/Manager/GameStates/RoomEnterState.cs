
using System;
using hvvan;
using UnityEngine;

public class RoomEnterState: IGameState
{
    private RoomController _currentRoomController;
    
    public void OnEnter()
    {
        //TODO: EnemyController로 클리어 판정 로직 필요
        _currentRoomController = RoomSceneController.Instance.CurrentRoomController;
        if (_currentRoomController)
        {
            _currentRoomController.OnClearSpotReached += ClearRoom;
        }
    }

    private void ClearRoom()
    {
        if (!_currentRoomController)
        {
            _currentRoomController = RoomSceneController.Instance.CurrentRoomController;
        }
        
        _currentRoomController.OnClearSpotReached -= ClearRoom;
        _currentRoomController.ClearRoom();
        
        GameManager.Instance.ChangeGameState(GameState.RoomClear);
    }

    //*TEST: 임시 클리어 판정
    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearRoom();
        }
    }

    public void OnExit()
    {
        _currentRoomController = null;
    }
}