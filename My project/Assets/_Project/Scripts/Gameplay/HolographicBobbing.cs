using UnityEngine;

namespace SpaceshipEscapeRoom.Gameplay
{
    [DisallowMultipleComponent]
    public class HolographicBobbing : MonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [Header("Bobbing")]
        [SerializeField] private bool _bobEnabled = true;
        [SerializeField, Min(0f)] private float _amplitude = 0.01f;
        [SerializeField, Min(0f)] private float _frequency = 1.5f;

        [Header("Rotation")]
        [SerializeField] private bool _rotateEnabled = true;
        [SerializeField] private float _rotationSpeed = 30f;

        private Vector3 _restLocalPosition;
        private float _phase;

        private void Awake()
        {
            _restLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _phase = 0f;
        }

        private void OnDisable()
        {
            transform.localPosition = _restLocalPosition;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (_bobEnabled)
            {
                // One full sine cycle is 2*pi radians, so the phase advances by 2*pi*frequency per second.
                // Wrapping it keeps the value small, which avoids float precision loss in long sessions.
                _phase += TwoPi * _frequency * deltaTime;
                if (_phase > TwoPi)
                {
                    _phase -= TwoPi;
                }

                Vector3 position = _restLocalPosition;
                position.y += Mathf.Sin(_phase) * _amplitude;
                transform.localPosition = position;
            }

            if (_rotateEnabled)
            {
                transform.Rotate(0f, _rotationSpeed * deltaTime, 0f, Space.Self);
            }
        }
    }
}
