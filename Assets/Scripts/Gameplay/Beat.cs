#nullable enable

using System;
using Events;
using SGS29.Utilities;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class Beat
    {
        public int Index;
        public BeatAction Action;
        public float NormalisedTime;
        public float TimeBuffer;
        public States State;
        
        
        public Beat(BeatAction action, float normalisedTime, float timeBuffer, States state, int index)
        {
            Action = action;
            NormalisedTime = normalisedTime;
            TimeBuffer = timeBuffer;
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
            if (state == States.Upcoming) return;
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