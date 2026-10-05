using System;
using SpaceshipEscapeRoom.AR;
using UnityEngine;

namespace SpaceshipEscapeRoom.Gameplay
{
    public enum GameState
    {
        Playing,
        Victory,
        Defeat
    }

    [DisallowMultipleComponent]
    public class EscapeGameManager : MonoBehaviour
    {
        public static EscapeGameManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private ARSessionController _sessionController;
        [SerializeField] private CardTrackingHandler _cardTracking;
        [SerializeField] private DepartmentKeypadController[] _keypads;

        [Header("Rules")]
        [SerializeField, Min(1f)] private float _countdownSeconds = 600f;
        [SerializeField] private string _masterPin = "7429";
        [SerializeField] private bool _requireAllDepartments = true;

        private readonly bool[] _solved = new bool[DepartmentInfo.Count];
        private readonly int[] _revealedDigits = new int[DepartmentInfo.Count];

        private GameState _state = GameState.Playing;
        private float _timeRemaining;
        private int _lastPublishedSeconds = -1;
        private int _solvedCount;

        public event Action<GameState> StateChanged;
        public event Action<int> TimerTicked;
        public event Action<string, int> DepartmentUnlocked;
        public event Action GameReset;

        public GameState State => _state;
        public int SecondsRemaining => Mathf.CeilToInt(_timeRemaining);
        public int SolvedCount => _solvedCount;
        public bool AllDepartmentsSolved => _solvedCount >= DepartmentInfo.Count;
        public int MasterPinLength => string.IsNullOrEmpty(_masterPin) ? 0 : _masterPin.Length;
        public bool CanSubmitMasterPin => _state == GameState.Playing && (!_requireAllDepartments || AllDepartmentsSolved);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _timeRemaining = _countdownSeconds;
        }

        private void OnEnable()
        {
            if (_cardTracking != null)
            {
                _cardTracking.CardDetected += HandleCardVisible;
                _cardTracking.CardUpdated += HandleCardVisible;
                _cardTracking.CardLost += HandleCardLost;
            }

            if (_keypads == null)
            {
                return;
            }

            for (int i = 0; i < _keypads.Length; i++)
            {
                if (_keypads[i] != null)
                {
                    _keypads[i].OnDepartmentUnlocked += HandleDepartmentUnlocked;
                }
            }
        }

        private void OnDisable()
        {
            if (_cardTracking != null)
            {
                _cardTracking.CardDetected -= HandleCardVisible;
                _cardTracking.CardUpdated -= HandleCardVisible;
                _cardTracking.CardLost -= HandleCardLost;
            }

            if (_keypads == null)
            {
                return;
            }

            for (int i = 0; i < _keypads.Length; i++)
            {
                if (_keypads[i] != null)
                {
                    _keypads[i].OnDepartmentUnlocked -= HandleDepartmentUnlocked;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            PublishTimer();
        }

        private void Update()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                PublishTimer();
                SetState(GameState.Defeat);
                return;
            }

            PublishTimer();
        }

        public bool IsDepartmentSolved(Department department)
        {
            return _solved[(int)department];
        }

        public bool TryGetRevealedDigit(Department department, out int digit)
        {
            digit = _revealedDigits[(int)department];
            return _solved[(int)department];
        }

        public bool TrySubmitMasterPin(PinEntry entry)
        {
            if (entry == null || !CanSubmitMasterPin || !entry.Matches(_masterPin))
            {
                return false;
            }

            SetState(GameState.Victory);
            return true;
        }

        public void ResetGame()
        {
            _timeRemaining = _countdownSeconds;
            _lastPublishedSeconds = -1;
            _solvedCount = 0;

            for (int i = 0; i < DepartmentInfo.Count; i++)
            {
                _solved[i] = false;
                _revealedDigits[i] = 0;
            }

            if (_keypads != null)
            {
                for (int i = 0; i < _keypads.Length; i++)
                {
                    if (_keypads[i] != null)
                    {
                        _keypads[i].ResetState();
                    }
                }
            }

            _state = GameState.Playing;

            if (_cardTracking != null)
            {
                _cardTracking.ClearActiveCards();
            }

            if (_sessionController != null)
            {
                _sessionController.ResetSession();
            }

            GameReset?.Invoke();
            StateChanged?.Invoke(_state);
            PublishTimer();
        }

        // The timer event only fires when the whole-second value changes, so listeners can
        // redraw text without doing any work on the frames in between.
        private void PublishTimer()
        {
            // Rounding up keeps 00:01 on screen until the countdown has really run out.
            int seconds = Mathf.CeilToInt(_timeRemaining);
            if (seconds == _lastPublishedSeconds)
            {
                return;
            }

            _lastPublishedSeconds = seconds;
            TimerTicked?.Invoke(seconds);
        }

        private void SetState(GameState newState)
        {
            if (_state == newState)
            {
                return;
            }

            _state = newState;

            if (_state != GameState.Playing)
            {
                HideAllKeypads();
            }

            StateChanged?.Invoke(_state);
        }

        private void HandleCardVisible(string cardName, Transform anchor)
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            DepartmentKeypadController keypad = FindKeypad(cardName);
            if (keypad != null)
            {
                keypad.PlaceAt(anchor);
            }
        }

        private void HandleCardLost(string cardName, Transform anchor)
        {
            DepartmentKeypadController keypad = FindKeypad(cardName);
            if (keypad != null)
            {
                keypad.Hide();
            }
        }

        private void HandleDepartmentUnlocked(string departmentId, int revealedDigit)
        {
            if (_keypads == null)
            {
                return;
            }

            for (int i = 0; i < _keypads.Length; i++)
            {
                DepartmentKeypadController keypad = _keypads[i];
                if (keypad == null || keypad.State != KeypadState.Solved)
                {
                    continue;
                }

                int index = (int)keypad.Department;
                if (_solved[index])
                {
                    continue;
                }

                _solved[index] = true;
                _revealedDigits[index] = keypad.RevealDigit;
                _solvedCount++;
            }

            DepartmentUnlocked?.Invoke(departmentId, revealedDigit);
        }

        private DepartmentKeypadController FindKeypad(string cardName)
        {
            if (_keypads == null)
            {
                return null;
            }

            for (int i = 0; i < _keypads.Length; i++)
            {
                if (_keypads[i] != null && _keypads[i].CardName == cardName)
                {
                    return _keypads[i];
                }
            }

            return null;
        }

        private void HideAllKeypads()
        {
            if (_keypads == null)
            {
                return;
            }

            for (int i = 0; i < _keypads.Length; i++)
            {
                if (_keypads[i] != null)
                {
                    _keypads[i].Hide();
                }
            }
        }
    }
}
