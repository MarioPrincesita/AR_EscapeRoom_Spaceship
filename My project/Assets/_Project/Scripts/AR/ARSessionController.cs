using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SpaceshipEscapeRoom.AR
{
    [DisallowMultipleComponent]
    public class ARSessionController : MonoBehaviour
    {
        [SerializeField] private ARSession _session;

        [Tooltip("Stop the AR session while the app is unfocused (system dialogs, notification shade). " +
                 "Off by default because the camera permission dialog also takes focus during startup.")]
        [SerializeField] private bool _pauseOnFocusLost;

        private bool _pausedByUser;
        private bool _pausedByFocus;
        private bool _applicationPaused;

        public event Action SessionPaused;
        public event Action SessionResumed;
        public event Action SessionReset;
        public event Action<ARSessionState> SessionStateChanged;
        public event Action<bool> ApplicationPauseChanged;

        public bool IsPaused => _session == null || !_session.enabled;
        public bool IsApplicationPaused => _applicationPaused;
        public ARSessionState State => ARSession.state;

        private void Awake()
        {
            if (_session == null)
            {
                _session = GetComponent<ARSession>();
            }

            if (_session == null)
            {
                Debug.LogError("ARSessionController: no ARSession assigned or found on this GameObject.", this);
            }
        }

        private void OnEnable()
        {
            ARSession.stateChanged += HandleSessionStateChanged;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= HandleSessionStateChanged;
        }

        public void PauseSession()
        {
            _pausedByUser = true;
            ApplySessionEnabled();
        }

        public void ResumeSession()
        {
            _pausedByUser = false;
            ApplySessionEnabled();
        }

        public void ResetSession()
        {
            if (_session == null)
            {
                Debug.LogWarning("ARSessionController: cannot reset, no ARSession assigned.", this);
                return;
            }

            _session.Reset();
            SessionReset?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // ARSession already forwards pause/resume to the ARCore subsystem, so the session is not toggled here.
            // Toggling it would stop and restart the subsystem and drop tracking on every background/foreground.
            if (_applicationPaused == pauseStatus)
            {
                return;
            }

            _applicationPaused = pauseStatus;
            ApplicationPauseChanged?.Invoke(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_pauseOnFocusLost)
            {
                return;
            }

            _pausedByFocus = !hasFocus;
            ApplySessionEnabled();
        }

        private void ApplySessionEnabled()
        {
            if (_session == null)
            {
                return;
            }

            bool shouldRun = !_pausedByUser && !_pausedByFocus;
            if (_session.enabled == shouldRun)
            {
                return;
            }

            if (shouldRun && ARSession.state == ARSessionState.Unsupported)
            {
                Debug.LogWarning("ARSessionController: AR is not supported on this device, session stays paused.", this);
                return;
            }

            _session.enabled = shouldRun;

            if (shouldRun)
            {
                SessionResumed?.Invoke();
            }
            else
            {
                SessionPaused?.Invoke();
            }
        }

        private void HandleSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            SessionStateChanged?.Invoke(args.state);
        }
    }
}
