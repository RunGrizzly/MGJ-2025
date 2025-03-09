using Events;
using SGS29.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

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
            _eventManager.RegisterListener<TrackEvents.TrackStarted>(OnTrackStarted);
            _eventManager.RegisterListener<TrackEvents.TrackFailed>(OnTrackFailed);
            _eventManager.RegisterListener<LevelPassed>(OnLevelPassed);
        }

        private void OnLevelPassed(LevelPassed obj)
        {
            _eventManager.UnregisterListener<TrackEvents.TrackFailed>(OnTrackFailed);
        }

        private void OnTrackStarted(TrackEvents.TrackStarted evt)
        {
            _playableTrack = evt.Track;
            _remainingAttempts = _maxAttempts;
        }

        private void OnTrackFailed(TrackEvents.TrackFailed evt)
        {
            _remainingAttempts-=1;
            _playableTrack.Reset();
            
            
            if (_remainingAttempts == 0)
            {
                _eventManager.DispatchEvent(new GameOver());
            }
        }
    }
}