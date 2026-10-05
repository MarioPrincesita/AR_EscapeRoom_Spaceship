using System;
using TMPro;
using UnityEngine;

namespace SpaceshipEscapeRoom.Gameplay
{
    public enum KeypadState
    {
        Locked,
        Solved
    }

    [DisallowMultipleComponent]
    public class DepartmentKeypadController : MonoBehaviour
    {
        private const string DeniedMessage = "DENIED";

        [SerializeField] private Department _department;
        [SerializeField] private string _correctPin = "1234";
        [SerializeField, Range(0, 9)] private int _revealDigit;

        [Header("Hologram")]
        [SerializeField] private GameObject _modelRoot;
        [SerializeField] private GameObject _keypadRoot;
        [SerializeField] private GameObject _revealRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private TMP_Text _revealText;
        [SerializeField] private TMP_Text _revealTextBack;

        private PinEntry _entry;
        private KeypadState _state = KeypadState.Locked;
        private bool _isPlaced;

        public event Action<string, int> OnDepartmentUnlocked;

        public Department Department => _department;
        public string DepartmentId => DepartmentInfo.GetId(_department);
        public string CardName => DepartmentInfo.GetCardName(_department);
        public KeypadState State => _state;
        public int RevealDigit => _revealDigit;
        public bool IsPlaced => _isPlaced;

        private void Awake()
        {
            int pinLength = string.IsNullOrEmpty(_correctPin) ? 4 : _correctPin.Length;
            _entry = new PinEntry(pinLength);

            if (_titleText != null)
            {
                _titleText.SetText(DepartmentId);
            }

            if (_revealText != null)
            {
                _revealText.SetText("{0}", _revealDigit);
            }

            // The digit spins, so a second copy faces the other way to keep it readable from behind.
            if (_revealTextBack != null)
            {
                _revealTextBack.SetText("{0}", _revealDigit);
            }

            _entry.ShowOn(_displayText);
            RefreshVisuals();
        }

        // Snaps the hologram onto the tracked card. The keypad and the reveal digit keep
        // their own local offsets, so only the root has to follow the card pose.
        public void PlaceAt(Transform anchor)
        {
            if (anchor == null)
            {
                return;
            }

            transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            if (!_isPlaced)
            {
                _isPlaced = true;
                RefreshVisuals();
            }
        }

        public void Hide()
        {
            if (!_isPlaced)
            {
                return;
            }

            _isPlaced = false;
            RefreshVisuals();
        }

        public void PressDigit(int digit)
        {
            if (_state != KeypadState.Locked || _entry == null)
            {
                return;
            }

            _entry.Append(digit);
            _entry.ShowOn(_displayText);
        }

        public void PressClear()
        {
            if (_state != KeypadState.Locked || _entry == null)
            {
                return;
            }

            _entry.Clear();
            _entry.ShowOn(_displayText);
        }

        public void PressEnter()
        {
            if (_state != KeypadState.Locked || _entry == null)
            {
                return;
            }

            if (!_entry.Matches(_correctPin))
            {
                _entry.Clear();
                if (_displayText != null)
                {
                    _displayText.SetText(DeniedMessage);
                }

                return;
            }

            _state = KeypadState.Solved;
            RefreshVisuals();
            OnDepartmentUnlocked?.Invoke(DepartmentId, _revealDigit);
        }

        public void ResetState()
        {
            _state = KeypadState.Locked;
            _isPlaced = false;

            if (_entry != null)
            {
                _entry.Clear();
                _entry.ShowOn(_displayText);
            }

            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (_modelRoot != null)
            {
                _modelRoot.SetActive(_isPlaced);
            }

            if (_keypadRoot != null)
            {
                _keypadRoot.SetActive(_isPlaced && _state == KeypadState.Locked);
            }

            if (_revealRoot != null)
            {
                _revealRoot.SetActive(_isPlaced && _state == KeypadState.Solved);
            }
        }
    }
}
