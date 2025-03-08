using Events;
using SGS29.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class AttemptSystem : MonoBehaviour
    {
        [SerializeField] private int _maxAttempts = 3;
        [SerializeField] private int _currentAttempt = 0;
        private PlayableTrack _playableTrack;
        private EventManager _eventManager;

        public void OnEnable()
        {
            _eventManager = SM.Instance<EventManager>();
            _eventManager.RegisterListener<TrackEvents.TrackStarted>(OnTrackStarted);
            _eventManager.RegisterListener<TrackEvents.TrackFailed>(OnTrackFailed);
        }

        private void OnTrackStarted(TrackEvents.TrackStarted evt)
        {
            _playableTrack = evt.Track;
            _currentAttempt = 0;
        }

        private void OnTrackFailed(TrackEvents.TrackFailed evt)
        {
            _currentAttempt++;
            _playableTrack.Reset();
            if (_currentAttempt >= _maxAttempts)
            {
                _eventManager.DispatchEvent(new GameOver());
            }
        }
    }
}