using System.Linq;
using Events;
using SGS29.Utilities;
using UnityEngine;

namespace Gameplay
{
    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField] private float _reactionWindow;
        [SerializeField] private float _rate;
        public PlayableTrack _currentTrack { get; private set; }
        private InputSystem_Actions _actions;
        private bool _isPlaying;
        private float _progress;
        private bool _isInSegment;
        private EventManager _eventManager;

        public void Play(PlayableTrack track)
        {
            _actions = new InputSystem_Actions();
            _actions.Ship.Enable();
            _currentTrack = track;
            // _currentTrack = PlayableTrack.FromTrackDefinition(trackDefinition, _rate, _reactionWindow);
            _actions.Ship.Action1.performed += _ => OnAction(BeatAction.Action1);
            _actions.Ship.Action2.performed += _ => OnAction(BeatAction.Action2);
            _actions.Ship.Action3.performed += _ => OnAction(BeatAction.Action3);
            _actions.Ship.Action4.performed += _ => OnAction(BeatAction.Action4);

            _progress = 0;
            _isPlaying = true;
            _currentTrack.SetState(PlayableTrack.States.Playing);
            _eventManager = SM.Instance<EventManager>();
            _eventManager.DispatchEvent(new TrackEvents.TrackStarted(_currentTrack));
        }

        private void OnAction(BeatAction action)
        {
            if (_currentTrack.State != PlayableTrack.States.Playing)
            {
                return;
            }

            if (_currentTrack.CurrentBeat != null)
            {
                if (action == _currentTrack.CurrentBeat.Action)
                {
                    _currentTrack.CurrentBeat.SetState(Beat.States.Success);
                    Debug.Log("HIT THE BEAT");
                }
                else
                {
                    _currentTrack.CurrentBeat.SetState(Beat.States.Failed);
                    Debug.Log("FUCKED THE BEAT");
                }
            }
            else
            {
                _currentTrack.SetState(PlayableTrack.States.Failed);
                Debug.Log("FUCKED THE BEAT 2");
            }

            UpdateTrackState();
        }

        private void UpdateTrackState()
        {
            if (_currentTrack.State != PlayableTrack.States.Playing)
            {
                return;
            }

            var beatsNotHit = _currentTrack.Beats.Where(beat =>
                beat.Action != BeatAction.Empty && beat.State != Beat.States.Success);
            if (!beatsNotHit.Any())
            {
                _currentTrack.SetState(PlayableTrack.States.Passed);
                _eventManager.DispatchEvent(new TrackEvents.TrackPassed(_currentTrack));
            }
        }

        public void Tick(float progress)
        {
            _progress = progress;
            var previousSegment = _currentTrack.CurrentBeat;
            _currentTrack.SetProgress(_progress);
            var currentSegment = _currentTrack.CurrentBeat;

            if (previousSegment != null
                && currentSegment != previousSegment
                && previousSegment.Action != BeatAction.Empty
                && previousSegment.State != Beat.States.Success)
            {
                previousSegment.SetState(Beat.States.Missed);
                Debug.Log("MISSED THE BEAT");
            }

            UpdateTrackState();

            if (_progress >= _currentTrack.Duration)
            {
                if (_currentTrack.State == PlayableTrack.States.Passed)
                {
                    // Debug.Log("PASSED TRACK");
                    // var trackEvent = new TrackEvents.TrackPassed(_currentTrack);
                    // _eventManager.DispatchEvent(trackEvent);
                }
                else
                {
                    _currentTrack.Reset();
                    var trackEvent = new TrackEvents.TrackFailed(_currentTrack);
                    _eventManager.DispatchEvent(trackEvent);
                }
            }
        }
    }

    namespace TrackEvents
    {
        public class TrackStarted : IEvent
        {
            public PlayableTrack Track { get; }

            public TrackStarted(PlayableTrack track)
            {
                Track = track;
            }
        }

        public class TrackPassed : IEvent
        {
            public PlayableTrack Track { get; }

            public TrackPassed(PlayableTrack track)
            {
                Track = track;
            }
        }

        public class TrackFailed : IEvent
        {
            public PlayableTrack Track { get; }

            public TrackFailed(PlayableTrack track)
            {
                Track = track;
            }
        }
    }
}