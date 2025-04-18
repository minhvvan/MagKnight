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
        public PlayerController Player { get; private set; }

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
            //데이터 load
            _playerData = SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData);
            if (_playerData == null)
            {
                Debug.Log($"{Constants.PlayerData} is null");
                //TODO: 생성후 저장
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
            
            //State 생성
            _states[GameState.Title] = new TitleState();
            _states[GameState.Loading] = new LoadingState();
            _states[GameState.BaseCamp] = new BaseCampState();
            _states[GameState.Run] = new RunState();
            _states[GameState.Dialogue] = new DialogueState();
            _states[GameState.Pause] = new PauseState();
            _states[GameState.Combat] = new CombatState();
            _states[GameState.GameClear] = new GameClearState();
            _states[GameState.GameOver] = new GameOverState();

            ChangeGameState(GameState.Title);
        }

        private void Start()
        {
            Player = FindObjectOfType<PlayerController>();
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

        public bool ChangeGameState(GameState newState)
        {
            if (!VerifyChangeState(newState)) return false;

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
            
            return true;
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
    }
}

