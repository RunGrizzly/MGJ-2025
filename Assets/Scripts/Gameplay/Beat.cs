#nullable enable

using Events;
using SGS29.Utilities;
using UnityEngine;

namespace Gameplay
{
    public class Beat
    {
        public int Index { get; }
        public BeatAction Action { get; }
        public float StartTime { get; }
        public float EndTime { get; }
        public States State { get; private set; }

        public Beat(BeatAction action, float startTime, float endTime, States state, int index)
        {
            Action = action;
            StartTime = startTime;
            EndTime = endTime;
            State = state;
            Index = index;
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

            if (state == States.Upcoming) return;

            var beatEvent = new BeatAttemptEvent(this);
            //SM.Instance<EventManager>().DispatchEvent(beatEvent);
        }
    }

    public class BeatAttemptEvent : IEvent
    {
        public Beat Beat { get; }

        public BeatAttemptEvent(Beat beat)
        {
            Beat = beat;
        }
    }
}