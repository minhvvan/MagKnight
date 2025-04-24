using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Jun;
using Managers;
using Moon;
using UnityEngine;

namespace hvvan
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] GameObject playerCharacterPrefab;
        
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

        public CurrentRunData CurrentRunData
        {
            get { return _currentRunData ??= new CurrentRunData(); }
            set => _currentRunData = value;
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
            _states[GameState.DungeonEnter] = new DungeonEnterState();
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
                loadData = await CreatePlayerData();
                await SaveDataManager.Instance.SaveData(Constants.PlayerData, loadData);
            }
            
            _playerData = loadData;
        }

        private async UniTask<PlayerData> CreatePlayerData()
        {
            Debug.Log($"Create PlayerData");
            var statSO = await DataManager.Instance.LoadDataAsync<PlayerStatSO>(Addresses.Data.Player.Stat);
    
            // 새 PlayerData 생성
            var playerData = new PlayerData
            {
                PlayerStat = statSO.Stat,
                Currency = 0
            };
    
            return playerData;
        }

        public void SetCurrentRunData(CurrentRunData currentRunData = null)
        {
            currentRunData ??= new CurrentRunData
            {
                playerStat = _playerData.PlayerStat
            };
            
            _currentRunData = currentRunData;
            
            SaveData(Constants.CurrentRun);
        }

        public void SaveData(string key)
        {
            if (key == Constants.PlayerData)
            {
                _ = SaveDataManager.Instance.SaveData(Constants.PlayerData, _playerData);
            }
            else if (key == Constants.CurrentRun)
            {
                if (!Player) return;
                
                if (Player.GetComponent<AbilitySystem>().TryGetAttributeSet<PlayerAttributeSet>(out var attributeSet))
                {
                    _currentRunData.playerStat = attributeSet.GetDataStruct();
                }

                _currentRunData.currentWeapon = Player.WeaponHandler.CurrentWeaponType;
                _ = SaveDataManager.Instance.SaveData(Constants.CurrentRun, _currentRunData);
            }
        }

        public void DeleteData(string key)
        {
            if (key == Constants.PlayerData)
            {
                SaveDataManager.Instance.DeleteData(Constants.PlayerData);
                _playerData = null;
            }
            else if (key == Constants.CurrentRun)
            {
                SaveDataManager.Instance.DeleteData(Constants.CurrentRun);
                _currentRunData = null;
            }
        }

        public async UniTask<PlayerStat> GetPlayerStat()
        {
            if (_playerData == null)
            {
                _playerData = SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData);
                await SetPlayerData(_playerData);
            }

            return _playerData.PlayerStat;
        }

        public PlayerStat GetCurrentStat()
        {
            _currentRunData ??= SaveDataManager.Instance.LoadData<CurrentRunData>(Constants.CurrentRun);
            return _currentRunData?.playerStat;
        }
    }
}

