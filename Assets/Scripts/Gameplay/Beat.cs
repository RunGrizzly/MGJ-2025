#nullable enable

namespace Gameplay
{
    public class Beat
    {
        public BeatAction Action { get; }
        public float StartTime { get; }
        public float EndTime { get; }
        public States State { get; private set; }

        public Beat(BeatAction action, float startTime, float endTime, States state)
        {
            Action = action;
            StartTime = startTime;
            EndTime = endTime;
            State = state;
        }

        public enum States
        {
            Upcoming,
            Missed,
            InProgress,
            Success,
            Failed
        }

        public void SetState(States state)
        {
            State = state;
        }
    }
}