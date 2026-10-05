using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SpaceshipEscapeRoom.AR
{
    [DisallowMultipleComponent]
    public class CardTrackingHandler : MonoBehaviour
    {
        public const string CardCommander = "Card_Commander";
        public const string CardEngineering = "Card_Engineering";
        public const string CardScience = "Card_Science";
        public const string CardNavigation = "Card_Navigation";

        [SerializeField] private ARTrackedImageManager _trackedImageManager;

        private readonly Dictionary<TrackableId, string> _activeCards = new Dictionary<TrackableId, string>(4);

        public event Action<string, Transform> CardDetected;
        public event Action<string, Transform> CardUpdated;
        public event Action<string, Transform> CardLost;

        public int ActiveCardCount => _activeCards.Count;

        private void Awake()
        {
            if (_trackedImageManager == null)
            {
                _trackedImageManager = GetComponent<ARTrackedImageManager>();
            }

            if (_trackedImageManager == null)
            {
                Debug.LogError("CardTrackingHandler: no ARTrackedImageManager assigned or found on this GameObject.", this);
            }
        }

        private void OnEnable()
        {
            if (_trackedImageManager != null)
            {
                _trackedImageManager.trackablesChanged.AddListener(HandleTrackablesChanged);
            }
        }

        private void OnDisable()
        {
            if (_trackedImageManager != null)
            {
                _trackedImageManager.trackablesChanged.RemoveListener(HandleTrackablesChanged);
            }

            ClearActiveCards();
        }

        // Reports every active card as lost and forgets it. Needed when listening stops and
        // before an AR session reset, because the session hands out new trackable ids afterwards.
        public void ClearActiveCards()
        {
            foreach (KeyValuePair<TrackableId, string> card in _activeCards)
            {
                CardLost?.Invoke(card.Value, null);
            }

            _activeCards.Clear();
        }

        public bool IsCardActive(string cardName)
        {
            foreach (KeyValuePair<TrackableId, string> card in _activeCards)
            {
                if (card.Value == cardName)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            for (int i = 0; i < args.added.Count; i++)
            {
                ProcessImage(args.added[i]);
            }

            for (int i = 0; i < args.updated.Count; i++)
            {
                ProcessImage(args.updated[i]);
            }

            for (int i = 0; i < args.removed.Count; i++)
            {
                KeyValuePair<TrackableId, ARTrackedImage> removed = args.removed[i];
                Transform lastTransform = removed.Value != null ? removed.Value.transform : null;
                ReleaseCard(removed.Key, lastTransform);
            }
        }

        private void ProcessImage(ARTrackedImage image)
        {
            if (image == null)
            {
                return;
            }

            // ARCore rarely removes an image that leaves the camera view. It keeps reporting it
            // as updated with a Limited tracking state, so that state is what marks a card as lost.
            if (image.trackingState != TrackingState.Tracking)
            {
                ReleaseCard(image.trackableId, image.transform);
                return;
            }

            if (_activeCards.TryGetValue(image.trackableId, out string cardName))
            {
                CardUpdated?.Invoke(cardName, image.transform);
                return;
            }

            cardName = image.referenceImage.name;
            if (string.IsNullOrEmpty(cardName))
            {
                return;
            }

            _activeCards.Add(image.trackableId, cardName);
            CardDetected?.Invoke(cardName, image.transform);
        }

        private void ReleaseCard(TrackableId id, Transform lastTransform)
        {
            if (!_activeCards.TryGetValue(id, out string cardName))
            {
                return;
            }

            _activeCards.Remove(id);
            CardLost?.Invoke(cardName, lastTransform);
        }
    }
}
