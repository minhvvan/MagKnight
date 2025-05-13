using System;
using System.Collections;
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
        
        private float _sessionStartTime;
        
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

        public CurrentRunData CurrentRunData => _currentRunData;

        private PlayerController _playerController;

        private CurrentRunData _currentRunData;
        private PlayerData _playerData;
        
        private GameState _previousGameState;
        private GameState _currentState;
        public GameState CurrentGameState => _currentState;
        
        private Dictionary<GameState, List<Action>> _stateListeners = new Dictionary<GameState, List<Action>>();
        private Dictionary<GameState, IGameState> _states = new Dictionary<GameState, IGameState>();
        public Action<GameState> GameStateChanged;
        
        //*임시 -> 변경 필요
        public Action OnMagneticPressed;
        public Action OnMagneticReleased;
        
        protected override void Initialize()
        {
            //State 생성
            _states[GameState.Title] = new TitleState();
            _states[GameState.InitGame] = new InitGameState();
            _states[GameState.BaseCamp] = new BaseCampState();
            _states[GameState.DungeonEnter] = new DungeonEnterState();
            _states[GameState.RoomEnter] = new RoomEnterState();
            _states[GameState.RoomClear] = new RoomClearState();
            _states[GameState.BossRoom]  = new BossRoomState();
            _states[GameState.Dialogue] = new DialogueState();
            _states[GameState.Pause] = new PauseState();
            _states[GameState.GameClear] = new GameClearState();
            _states[GameState.GameOver] = new GameOverState();

            ChangeGameState(GameState.Title);
        }

        private void Start()
        {
            _sessionStartTime = Time.unscaledTime;
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

            switch (_currentState)
            {
                case GameState.BaseCamp:
                    AudioManager.Instance.PlayBGM(AudioBase.BGM.BaseCamp.Main);
                    AudioManager.Instance.PlayBGMEnvironment(AudioBase.BGM.BaseCamp.Environment);
                    break;

                case GameState.DungeonEnter:
                    AudioManager.Instance.PlayBGM(AudioBase.BGM.Dungeon.Stage1.MainBattle);
                    AudioManager.Instance.PlayBGMEnvironment(AudioBase.BGM.Dungeon.Stage1.Environment);
                    break;
                case GameState.BossRoom:
                    AudioManager.Instance.PlayBGM(AudioBase.BGM.Dungeon.Stage1.Boss);
                    break;
                case GameState.GameClear:
                    AudioManager.Instance.PlayBGM(null);
                    AudioManager.Instance.PlaySFX(AudioBase.SFX.Room.StageClear);
                    break;
                case GameState.GameOver:
                    AudioManager.Instance.PlayBGM(null);
                    AudioManager.Instance.PlaySFX(AudioBase.SFX.Room.GameOver);
                    break;
            }

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
            var statSO = await DataManager.Instance.LoadScriptableObjectAsync<PlayerStatSO>(Addresses.Data.Player.Stat);
    
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
        }

        public async UniTask SaveData(string key)
        {
            if (key == Constants.PlayerData)
            {
                await SaveDataManager.Instance.SaveData(Constants.PlayerData, _playerData);
            }
            else if (key == Constants.CurrentRun)
            {
                if (!Player) return;
                
                if (Player.AbilitySystem.TryGetAttributeSet<PlayerAttributeSet>(out var attributeSet))
                {
                    _currentRunData.playerStat = await attributeSet.GetDataStruct();
                }

                if (_currentRunData.currentWeapon == WeaponType.None)
                {
                    _currentRunData.currentWeapon = Player.WeaponHandler.CurrentWeaponType;
                }

                _currentRunData.playTime += Time.unscaledTime - _sessionStartTime;
                _sessionStartTime = Time.unscaledTime;
                
                await SaveDataManager.Instance.SaveData(Constants.CurrentRun, _currentRunData);
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
            //이미 존재하면서 유효하면 반환
            if (_playerData != null && _playerData.PlayerStat.IsValid()) return _playerData.PlayerStat;
            
            //데이터 로드 시도
            _playerData = SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData);
            
            if (_playerData == null)
            {
                //로드 실패 -> 생성 및 저장
                _playerData = await CreatePlayerData();
                await SaveData(Constants.PlayerData);
            }
            else
            {
                //로드 성공 -> 유효성 검증
                if (!_playerData.PlayerStat.IsValid())
                {
                    //유효성 검증 실패 -> 삭제 및 재생성 
                    SaveDataManager.Instance.DeleteData(Constants.PlayerData);
                    _playerData = await CreatePlayerData();
                    await SaveData(Constants.PlayerData);
                }
            }
            
            await SetPlayerData(_playerData);
            return _playerData.PlayerStat;
        }
        
        public async UniTask<PlayerData> GetPlayerData()
        {
            //이미 존재하면서 유효하면 반환
            if (_playerData != null && _playerData.PlayerStat.IsValid()) return _playerData.DeepCopy();
            
            //데이터 로드 시도
            _playerData = SaveDataManager.Instance.LoadData<PlayerData>(Constants.PlayerData);
            
            if (_playerData == null)
            {
                //로드 실패 -> 생성 및 저장
                _playerData = await CreatePlayerData();
                await SaveData(Constants.PlayerData);
            }
            else
            {
                //로드 성공 -> 유효성 검증
                if (!_playerData.PlayerStat.IsValid())
                {
                    //유효성 검증 실패 -> 삭제 및 재생성 
                    SaveDataManager.Instance.DeleteData(Constants.PlayerData);
                    _playerData = await CreatePlayerData();
                    await SaveData(Constants.PlayerData);
                }
            }
            
            await SetPlayerData(_playerData);
            return _playerData.DeepCopy();
        }

        public PlayerStat GetCurrentStat()
        {
            return _currentRunData?.playerStat;
        }

        public async UniTask MoveToNextFloor()
        {
            CurrentRunData.currentFloor++;
            CurrentRunData.currentRoomIndex = 0;
            CurrentRunData.clearedRooms.Clear();
            CurrentRunData.seed = (int)DateTime.Now.Ticks % int.MaxValue;
            await SaveData(Constants.CurrentRun);
            
            var floorData = await DataManager.Instance.LoadScriptableObjectAsync<FloorDataSO>(Addresses.Data.Room.Floor);

            //다음층 시작룸 로드
            var startRoom = floorData.Floor[CurrentRunData.currentFloor].rooms[RoomType.StartRoom];
            SceneController.TransitionToScene(startRoom.sceneName, true, NextFloorSceneLoaded);
        }

        private IEnumerator NextFloorSceneLoaded()
        {
            var enterTask = RoomSceneController.Instance.EnterFloor();
            while (!enterTask.Status.IsCompleted())
            {
                yield return null;
            }

            var enterRoomTask = RoomSceneController.Instance.CurrentRoomController.OnPlayerEnter(RoomDirection.North, true);
            while (!enterRoomTask.Status.IsCompleted())
            {
                yield return null;
            }
            
            RoomSceneController.Instance.CurrentRoomController.SetRoomReady(true);
            
            //플레이어 설정
            if (Player)
            {
                Player.InitializeByCurrentRunData(CurrentRunData);
            }
        }
    }
}

