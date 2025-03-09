#nullable enable
using System.Linq;
using Events;
using Gameplay.TrackEvents;
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
        private float _progress;
        private bool _isInSegment;
        private EventManager _eventManager;

        private void OnEnable()
        {
            _actions = new InputSystem_Actions();
            _actions.Ship.Action1.performed += _ => OnAction(BeatAction.Action1);
            _actions.Ship.Action2.performed += _ => OnAction(BeatAction.Action2);
            _actions.Ship.Action3.performed += _ => OnAction(BeatAction.Action3);
            _actions.Ship.Action4.performed += _ => OnAction(BeatAction.Action4);
            _actions.Ship.Transfer.performed += _ => OnAction(BeatAction.Transfer);
        }

        public void Play(PlayableTrack track)
        {
            _actions.Ship.Enable();
            _currentTrack = track;
            _currentTrack.SetState(PlayableTrack.States.Playing);
            _currentTrack.SetProgress(_progress);
            _eventManager = SM.Instance<EventManager>();
            _eventManager.DispatchEvent(new TrackStarted(_currentTrack));
        }

        private void OnAction(BeatAction action)
        {
            if (_currentTrack.State != PlayableTrack.States.Playing)
            {
                return;
            }

            switch (_currentTrack.CurrentBeat)
            {
                case { State: Beat.States.InProgress } when action == _currentTrack.CurrentBeat.Action:
                    _currentTrack.CurrentBeat.SetState(Beat.States.Success);
                    Debug.Log("Hit the beat");
                    break;
                case { State: Beat.States.InProgress }:
                    _currentTrack.CurrentBeat.SetState(Beat.States.Failed);
                    Debug.Log("Tried to hit the wrong beat");
                    break;
                case null:
                    _currentTrack.SetState(PlayableTrack.States.Failed);
                    Debug.Log("Tried to hit a beat, but no beat");
                    break;
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
                _actions.Ship.Disable();
                _eventManager.DispatchEvent(new TrackPassed(_currentTrack));
            }
        }

        public void Tick(float progress)
        {
            _progress = progress;
            var previousSegment = _currentTrack.CurrentBeat;
            _currentTrack.SetProgress(_progress);
            var currentSegment = _currentTrack.CurrentBeat;

            if (DidMissABeat(previousSegment, currentSegment))
            {
                // previousSegment.SetState(Beat.States.Missed);
                // Debug.Log("Missed a beat");
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
                    var trackEvent = new TrackFailed(_currentTrack);
                    _eventManager.DispatchEvent(trackEvent);
                }
            }
        }

        private static bool DidMissABeat(Beat? previous, Beat? current)
        {
            var isDifferentBeats = previous == current;
            var wasPreviousBeatAction = previous != null && previous.Action != BeatAction.Empty;
            var wasPreviousBeatHit = previous is { State: Beat.States.Success };
            return isDifferentBeats && wasPreviousBeatAction && !wasPreviousBeatHit;
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