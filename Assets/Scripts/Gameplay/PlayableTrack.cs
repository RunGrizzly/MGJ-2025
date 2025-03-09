#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using SGS29.Utilities;
using UnityEngine;

public class TrackResetEvent : IEvent
{
}

namespace Gameplay
{
    [Serializable]
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

        public Beat? GetCurrentBeat()
        {
            // var beats = GetNormalizedBeatTimes();
            //
            var closestBeat = Beats.ElementAt(0);
            var shortestDistance = float.MaxValue;
            
            
            foreach (var beat in Beats)
            {
                var distance = ((beat.StartTime+beat.EndTime)/2) - _progress;

                if (distance < shortestDistance && distance >0)
                {
                    closestBeat = beat;
                    shortestDistance = distance;
                }

            }
            
            //var currentBeat= Beats.FirstOrDefault(beat => _progress >= beat.StartTime && _progress <= beat.EndTime);
            
            return closestBeat;
        }

        public static PlayableTrack FromTrackDefinition(TrackDefinition trackDefinition, float rate, float timingWindow)
        {
            var duration = trackDefinition.Actions.Count / rate;
            
            //Seconds of deadzone as a portion of the duration
            var deadZone = duration-active;
            
            //The actual active duration that we're working with that sits in the middle of the deadzones
            float activeDuration = duration - (deadZone);
           
            //Figure out the remaining interval that we will spawn into
            float interval = activeDuration /(trackDefinition.Actions.Count-1) ;
            
            var actions = trackDefinition.Actions.Select((action, i) => new Beat
            (
                action,
                (deadZone + (i * interval))-timingWindow,
                (deadZone + (i * interval))+timingWindow,
                Beat.States.Upcoming,
                i
            ));
            foreach (var beat in actions)
            {
                // Debug.Log($"Beat: {beat.Action} {beat.StartTime}-{beat.EndTime}");
            }

            return new PlayableTrack(actions.ToList(), duration);
        }

        public Dictionary<Beat, float> GetNormalizedBeatTimes()
        {
            var normalizedBeatTimes = new Dictionary<Beat, float>();
            
            foreach (var beat in Beats)
            {
                normalizedBeatTimes.Add(beat, ((beat.StartTime + beat.EndTime) / 2) / Duration);
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