using UnityEngine;
using UnityEngine.Polybrush;

namespace Gameplay
{
    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField] private OrbitManager _orbitManager;
        [SerializeField] private float _reactionWindow;
        [SerializeField] private float _rate;
        private InputSystem_Actions _actions;
        public PlayableTrack _currentTrack { get; private set; }
        private bool _isPlaying;
        private float _progress;
        private bool _isInSegment;

        public void Play(TrackDefinition trackDefinition)
        {
            _actions = new InputSystem_Actions();
            _actions.Ship.Enable();
            _currentTrack = PlayableTrack.FromTrackDefinition(trackDefinition, _rate, _reactionWindow);
            _actions.Ship.Action1.performed += _ => OnAction(BeatAction.Action1);
            _actions.Ship.Action2.performed += _ => OnAction(BeatAction.Action2);
            _actions.Ship.Action3.performed += _ => OnAction(BeatAction.Action3);
            _actions.Ship.Action4.performed += _ => OnAction(BeatAction.Action4);

            _progress = 0;
            _isPlaying = true;
        }

        private void OnAction(BeatAction action)
        {
            if (_currentTrack.CurrentAction != null)
            {
                if (action == _currentTrack.CurrentAction.Action)
                {
                    _currentTrack.CurrentAction.SetState(Beat.States.Success);
                    Debug.Log("HIT THE BEAT");
                }
                else
                {
                    _currentTrack.CurrentAction.SetState(Beat.States.Failed);
                    Debug.Log("FUCKED THE BEAT");
                }
            }
            else
            {
                _currentTrack.SetState(PlayableTrack.States.Failed);
                Debug.Log("FUCKED THE BEAT 2");
            }
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _progress += Time.deltaTime;
            _orbitManager.m_normalisedPosition = _progress / _currentTrack.Duration;

            var previousSegment = _currentTrack.CurrentAction;
            _currentTrack.SetProgress(_progress);
            var currentSegment = _currentTrack.CurrentAction;

            if (previousSegment != null && currentSegment != previousSegment &&
                previousSegment.State != Beat.States.Success)
            {
                previousSegment.SetState(Beat.States.Missed);
                Debug.Log("MISSED THE BEAT");
            }

            if (_progress >= _currentTrack.Duration)
            {
                if (_currentTrack.State == PlayableTrack.States.Passed)
                {
                    Debug.Log("PASSED TRACK");
                }
                else
                {
                    _progress = Mathf.Repeat(_progress, _currentTrack.Duration);
                    _currentTrack.Reset();
                }
            }
        }
    }
}