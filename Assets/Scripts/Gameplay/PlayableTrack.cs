#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace Gameplay
{
    public class PlayableTrack
    {
        public List<Beat> Actions { get; private set; }
        public Beat? CurrentAction { get; private set; }
        public float Duration { get; private set; }
        public States State { get; private set; }

        private float _progress;

        private PlayableTrack(List<Beat> actions, float duration)
        {
            Actions = actions;
            Duration = duration;
        }

        public void SetProgress(float progress)
        {
            _progress = progress;
            CurrentAction = GetCurrentAction();
            if (CurrentAction != null && CurrentAction.State == Beat.States.Upcoming)
            {
                CurrentAction.SetState(Beat.States.InProgress);
            }
        }

        private Beat? GetCurrentAction()
        {
            return Actions.FirstOrDefault(action => _progress >= action.StartTime && _progress <= action.EndTime);
        }

        public static PlayableTrack FromTrackDefinition(TrackDefinition trackDefinition, float rate, float timingWindow)
        {
            var actionSpacing = 1 / rate;
            var actions = trackDefinition.Actions.Select((action, i) => new Beat(
                action,
                i * actionSpacing,
                i * actionSpacing + timingWindow,
                Beat.States.Upcoming
            ));
            return new PlayableTrack(actions.ToList(), trackDefinition.Actions.Count * actionSpacing);
        }

        public void SetState(States state)
        {
            State = state;
        }


        public enum States
        {
            Playing,
            Failed,
            Passed,
        }
    }
}