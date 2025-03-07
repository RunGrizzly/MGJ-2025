using UnityEngine;

namespace Gameplay
{
    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField] private float _reactionWindow;
        [SerializeField] private float _rate;
        private InputSystem_Actions _actions;
        private PlayableTrack _currentTrack;
        private bool _isPlaying;
        private float _progress;
        private bool _isInSegment;

        public void Play(TrackDefinition trackDefinition)
        {
            _actions.Ship.Enable();
            _currentTrack = PlayableTrack.FromTrackDefinition(trackDefinition, _rate, _reactionWindow);
            _actions.Ship.Action1.performed += _ => OnAction(BeatAction.Action1);
            _actions.Ship.Action2.performed += _ => OnAction(BeatAction.Action2);
            _actions.Ship.Action3.performed += _ => OnAction(BeatAction.Action3);
            _actions.Ship.Action4.performed += _ => OnAction(BeatAction.Action4);
        }

        private void OnAction(BeatAction action)
        {
            if (_currentTrack.CurrentAction != null)
            {
                if (action == _currentTrack.CurrentAction.Action)
                {
                    _currentTrack.CurrentAction.SetState(Beat.States.Success);
                }
                else
                {
                    _currentTrack.CurrentAction.SetState(Beat.States.Failed);
                }
            }
            else
            {
                _currentTrack.SetState(PlayableTrack.States.Failed);
            }
        }

        private void Update()
        {
            if (_isPlaying)
            {
                _progress += Time.deltaTime * (1 / _rate);
            }

            var previousSegment = _currentTrack.CurrentAction;
            _currentTrack.SetProgress(_progress);
            var currentSegment = _currentTrack.CurrentAction;

            if (previousSegment != null && currentSegment != previousSegment &&
                previousSegment.State != Beat.States.Success)
            {
                previousSegment.SetState(Beat.States.Missed);
            }

            // Loop track
        }
    }
}