using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Managers;
using Moon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace hvvan
{
    public class GameManager : Singleton<GameManager>
    {
        public PlayerController Player
        {
            get
            {
                if (_playerController == null)
                {
                    return _playerController = FindObjectOfType<PlayerController>();
                }

                return _playerController;
            }
            private set => _playerController = value;
        }

        private PlayerController _playerController;

        private CurrentRunData _currentRunData;
        private PlayerData _playerData;
        
        private GameState _previousGameState;
        private GameState _currentState;
        public GameState CurrentGameState => _currentState;
        
        private Dictionary<GameState, List<Action>> _stateListeners = new Dictionary<GameState, List<Action>>();
        private Dictionary<GameState, IGameState> _states = new Dictionary<GameState, IGameState>();
        public Action<GameState> GameStateChanged;
        
        protected override void Initialize()
        {
            //State 생성
            _states[GameState.Title] = new TitleState();
            _states[GameState.InitGame] = new InitGameState();
            _states[GameState.BaseCamp] = new BaseCampState();
            _states[GameState.RoomEnter] = new RoomEnterState();
            _states[GameState.RoomClear] = new RoomClearState();
            _states[GameState.Dialogue] = new DialogueState();
            _states[GameState.Pause] = new PauseState();
            _states[GameState.GameClear] = new GameClearState();
            _states[GameState.GameOver] = new GameOverState();

            ChangeGameState(GameState.Title);
        }

        private void Start()
        {
            Player = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            if (_currentState != GameState.None)
            {
                _states[_currentState].OnUpdate();;
            }
        }

        public void AddStateListener(GameState state, Action callback)
        {
            if (!_stateListeners.ContainsKey(state))
            {
                _stateListeners[state] = new List<Action>();
            }
    
            _stateListeners[state].Add(callback);
        }

        public void RemoveStateListener(GameState state, Action callback)
        {
            if (_stateListeners.ContainsKey(state))
            {
                _stateListeners[state].Remove(callback);
            }
        }

        public void ChangeGameState(GameState newState)
        {
            if (!VerifyChangeState(newState)) return;

            if (_currentState != GameState.None)
            {
                _states[_currentState].OnExit();
            }

            _previousGameState = _currentState;
            _currentState = newState;

            _states[_currentState].OnEnter();
            GameStateChanged?.Invoke(_currentState);
    
            if (_stateListeners.TryGetValue(newState, out var stateListener))
            {
                foreach (var listener in stateListener)
                {
                    listener.Invoke();
                }
            }
            
            return;
        }

        private bool VerifyChangeState(GameState newState)
        {
            //current에서 state로 바꿀 수 있는지 검증
            return true;
        }

        public void RecoverPreviousState()
        {
            ChangeGameState(_previousGameState);
        }

        public async UniTask SetPlayerData(PlayerData loadData)
        {
            if (loadData == null)
            {
                Debug.Log($"Create PlayerData");
                loadData = new PlayerData();
                await SaveDataManager.Instance.SaveData(Constants.PlayerData, loadData);
            }
            
            _playerData = loadData;
        }

        public void SetCurrentRunData(CurrentRunData currentRunData)
        {
            _currentRunData = currentRunData;
        }
    }
}

