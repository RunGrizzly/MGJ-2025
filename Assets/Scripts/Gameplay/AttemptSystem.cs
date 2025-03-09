using Events;
using SGS29.Utilities;
using UnityEngine;

namespace Gameplay
{
    public class AttemptSystem : MonoBehaviour
    {
        [SerializeField] private int _maxAttempts = 3;
        public int _remainingAttempts = 3;
        
        private PlayableTrack _playableTrack;
        private EventManager _eventManager;

        public void OnEnable()
        {
            _eventManager = SM.Instance<EventManager>();
            _eventManager.RegisterListener<NewLevel>(OnTrackStarted);
            _eventManager.RegisterListener<LevelPassed>(OnLevelPassed);
        }

        private void OnLevelPassed(LevelPassed obj)
        {
            _eventManager.UnregisterListener<TrackEvents.TrackFailed>(OnNewLevel);
        }

        private void OnTrackStarted(NewLevel evt)
        {
            _playableTrack = evt.Level.Track;
            _remainingAttempts = _maxAttempts;
            _eventManager.RegisterListener<TrackEvents.TrackFailed>(OnNewLevel);
        }

        private void OnNewLevel(TrackEvents.TrackFailed evt)
        {
            _remainingAttempts--;
            Debug.Log($"Attempt failed, Remaining attempts: {_remainingAttempts}");
            _playableTrack.Reset();
            
            if (_remainingAttempts == 0)
            {
                _eventManager.DispatchEvent(new GameOver());
            }
        }
    }
}