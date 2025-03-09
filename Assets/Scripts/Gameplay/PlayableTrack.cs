#nullable enable
using System.Collections.Generic;
using System.Linq;
using Events;
using SGS29.Utilities;

public class TrackResetEvent : IEvent
{
}

namespace Gameplay
{
    public class PlayableTrack
    {
        public List<Beat> Beats { get; private set; }
        public Beat? CurrentBeat { get; private set; }
        public float Duration { get; private set; }
        public States State { get; private set; }

        private float _progress;

        public PlayableTrack(List<Beat> beats, float duration)
        {
            Beats = beats;
            Duration = duration;
        }

        public void SetProgress(float progress)
        {
            _progress = progress;
            CurrentBeat = GetCurrentBeat();

            if (CurrentBeat != null && CurrentBeat.State == Beat.States.Upcoming)
            {
                CurrentBeat.SetState(Beat.States.InProgress);
                // Debug.Log($"Beat Active: {CurrentBeat.Action}");
            }
        }

        public void Reset()
        {
            foreach (var beat in Beats)
            {
                beat.SetState(Beat.States.Upcoming);
            }
            
            SM.Instance<EventManager>().DispatchEvent(new TrackResetEvent());
        }

        private Beat? GetCurrentBeat()
        {
            return Beats.FirstOrDefault(action => _progress >= action.StartTime && _progress <= action.EndTime);
        }

        public static PlayableTrack FromTrackDefinition(TrackDefinition trackDefinition, float rate, float timingWindow)
        {
            var actionSpacing = 1 / rate;
            var actions = trackDefinition.Actions.Select((action, i) => new Beat(
                action,
                i * actionSpacing,
                i * actionSpacing + timingWindow,
                Beat.States.Upcoming,
                i
            ));
            foreach (var beat in actions)
            {
                // Debug.Log($"Beat: {beat.Action} {beat.StartTime}-{beat.EndTime}");
            }

            return new PlayableTrack(actions.ToList(), trackDefinition.Actions.Count * actionSpacing);
        }

        public Dictionary<Beat, float> GetNormalizedBeatTimes()
        {
            var normalizedBeatTimes = new Dictionary<Beat, float>();
            foreach (var beat in Beats)
            {
                normalizedBeatTimes.Add(beat, (beat.StartTime + beat.EndTime) / 2 / Duration);
                // Debug.Log($"KYLEMESS: Beat times: {beat.StartTime / Duration}");
            }

            return normalizedBeatTimes;
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