using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public PlayerController Player { get; private set; }

    private CurrentRunData _currentRunData;
    private PlayerData _playerData;

    protected override void Initialize()
    {
        //데이터 load
        _playerData = SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData);
        if (_playerData == null)
        {
            Debug.Log($"{Constants.PlayerData} is null");
            return;
        }

        _currentRunData = SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.PlayerData);
        if (_currentRunData == null)
        {
            //회차 정보 없음 => BaseCamp로
        }
        else
        {
            //회차 정보대로 씬 이동 및 설정
        }
    }

    private void Start()
    {
        Player = FindObjectOfType<PlayerController>();
    }
}
