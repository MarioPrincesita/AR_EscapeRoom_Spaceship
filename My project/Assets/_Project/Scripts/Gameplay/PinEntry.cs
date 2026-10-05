using TMPro;

namespace SpaceshipEscapeRoom.Gameplay
{
    // Fixed-size digit buffer shared by the card keypads and the master console.
    // Digits live in a char array so typing and redrawing never build new strings.
    public class PinEntry
    {
        private const char EmptySlot = '-';

        private readonly char[] _slots;

        public PinEntry(int capacity)
        {
            _slots = new char[capacity < 1 ? 1 : capacity];
            Clear();
        }

        public int Count { get; private set; }
        public int Capacity => _slots.Length;
        public bool IsFull => Count == _slots.Length;

        public bool Append(int digit)
        {
            if (digit < 0 || digit > 9 || IsFull)
            {
                return false;
            }

            _slots[Count] = (char)('0' + digit);
            Count++;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = EmptySlot;
            }

            Count = 0;
        }

        public bool Matches(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (_slots[i] != pin[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void ShowOn(TMP_Text label)
        {
            if (label != null)
            {
                label.SetCharArray(_slots, 0, _slots.Length);
            }
        }
    }
}
