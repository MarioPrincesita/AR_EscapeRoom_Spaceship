using SpaceshipEscapeRoom.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceshipEscapeRoom.UI
{
    [DisallowMultipleComponent]
    public class EscapeHUDController : MonoBehaviour
    {
        private const string LockedDigit = "?";
        private const string DeniedMessage = "ACCESS DENIED";
        private const string LockedMessage = "UNLOCK ALL DEPARTMENTS FIRST";

        [SerializeField] private EscapeGameManager _gameManager;

        [Header("Top Bar")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Image[] _departmentBadges = new Image[DepartmentInfo.Count];
        [SerializeField] private TMP_Text[] _departmentDigits = new TMP_Text[DepartmentInfo.Count];
        [SerializeField] private Color _lockedColor = new Color(0.55f, 0.12f, 0.12f, 0.9f);
        [SerializeField] private Color _unlockedColor = new Color(0.1f, 0.65f, 0.45f, 0.9f);

        [Header("Master Console")]
        [SerializeField] private Button _masterButton;
        [SerializeField] private GameObject _masterPanel;
        [SerializeField] private TMP_Text _masterDisplay;
        [SerializeField] private TMP_Text _masterStatus;

        [Header("End Screens")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;

        // "MM:SS" is rewritten in place so the timer never allocates a string.
        private readonly char[] _timerChars = { '0', '0', ':', '0', '0' };

        private PinEntry _masterEntry;

        private void Awake()
        {
            int pinLength = _gameManager != null ? _gameManager.MasterPinLength : 0;
            _masterEntry = new PinEntry(pinLength > 0 ? pinLength : 4);

            if (_gameManager == null)
            {
                Debug.LogError("EscapeHUDController: no EscapeGameManager assigned.", this);
            }
        }

        private void OnEnable()
        {
            if (_gameManager == null)
            {
                return;
            }

            _gameManager.TimerTicked += HandleTimerTicked;
            _gameManager.DepartmentUnlocked += HandleDepartmentUnlocked;
            _gameManager.StateChanged += HandleStateChanged;
            _gameManager.GameReset += HandleGameReset;
        }

        private void OnDisable()
        {
            if (_gameManager == null)
            {
                return;
            }

            _gameManager.TimerTicked -= HandleTimerTicked;
            _gameManager.DepartmentUnlocked -= HandleDepartmentUnlocked;
            _gameManager.StateChanged -= HandleStateChanged;
            _gameManager.GameReset -= HandleGameReset;
        }

        private void Start()
        {
            RefreshAll();
        }

        public void OpenMasterConsole()
        {
            if (_gameManager == null || _gameManager.State != GameState.Playing)
            {
                return;
            }

            _masterEntry.Clear();
            _masterEntry.ShowOn(_masterDisplay);
            SetMasterStatus(_gameManager.CanSubmitMasterPin ? string.Empty : LockedMessage);
            SetActive(_masterPanel, true);
        }

        public void CloseMasterConsole()
        {
            SetActive(_masterPanel, false);
        }

        public void PressMasterDigit(int digit)
        {
            _masterEntry.Append(digit);
            _masterEntry.ShowOn(_masterDisplay);
        }

        public void ClearMasterEntry()
        {
            _masterEntry.Clear();
            _masterEntry.ShowOn(_masterDisplay);
            SetMasterStatus(string.Empty);
        }

        public void SubmitMasterEntry()
        {
            if (_gameManager == null)
            {
                return;
            }

            if (!_gameManager.CanSubmitMasterPin)
            {
                SetMasterStatus(LockedMessage);
                return;
            }

            if (_gameManager.TrySubmitMasterPin(_masterEntry))
            {
                return;
            }

            _masterEntry.Clear();
            _masterEntry.ShowOn(_masterDisplay);
            SetMasterStatus(DeniedMessage);
        }

        public void RetryGame()
        {
            if (_gameManager != null)
            {
                _gameManager.ResetGame();
            }
        }

        private void HandleTimerTicked(int secondsRemaining)
        {
            if (_timerText == null)
            {
                return;
            }

            int minutes = Mathf.Min(secondsRemaining / 60, 99);
            int seconds = secondsRemaining % 60;

            _timerChars[0] = (char)('0' + minutes / 10);
            _timerChars[1] = (char)('0' + minutes % 10);
            _timerChars[3] = (char)('0' + seconds / 10);
            _timerChars[4] = (char)('0' + seconds % 10);
            _timerText.SetCharArray(_timerChars, 0, _timerChars.Length);
        }

        private void HandleDepartmentUnlocked(string departmentId, int revealedDigit)
        {
            RefreshBadges();
        }

        private void HandleStateChanged(GameState state)
        {
            RefreshPanels(state);
        }

        private void HandleGameReset()
        {
            _masterEntry.Clear();
            _masterEntry.ShowOn(_masterDisplay);
            SetMasterStatus(string.Empty);
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (_gameManager == null)
            {
                return;
            }

            HandleTimerTicked(_gameManager.SecondsRemaining);
            RefreshBadges();
            RefreshPanels(_gameManager.State);
        }

        private void RefreshBadges()
        {
            if (_gameManager == null)
            {
                return;
            }

            for (int i = 0; i < DepartmentInfo.Count; i++)
            {
                bool solved = _gameManager.TryGetRevealedDigit((Department)i, out int digit);

                if (i < _departmentBadges.Length && _departmentBadges[i] != null)
                {
                    _departmentBadges[i].color = solved ? _unlockedColor : _lockedColor;
                }

                if (i >= _departmentDigits.Length || _departmentDigits[i] == null)
                {
                    continue;
                }

                if (solved)
                {
                    _departmentDigits[i].SetText("{0}", digit);
                }
                else
                {
                    _departmentDigits[i].SetText(LockedDigit);
                }
            }
        }

        private void RefreshPanels(GameState state)
        {
            bool playing = state == GameState.Playing;

            SetActive(_victoryPanel, state == GameState.Victory);
            SetActive(_defeatPanel, state == GameState.Defeat);

            if (!playing)
            {
                SetActive(_masterPanel, false);
            }

            if (_masterButton != null)
            {
                _masterButton.gameObject.SetActive(playing);
            }
        }

        private void SetMasterStatus(string message)
        {
            if (_masterStatus != null)
            {
                _masterStatus.SetText(message);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
