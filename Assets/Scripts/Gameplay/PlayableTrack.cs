#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Gameplay;
using SGS29.Utilities;
using TrackEvents;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TrackResetEvent : IEvent
{
    public PlayableTrack Track;
    
    public TrackResetEvent(PlayableTrack track)
    {
        Track = track;
    }
}

namespace Gameplay
{
    public enum States
    {
        NotPlaying,
        MainTrack,
        ExitTrack,
        Failed,
        Passed,
    }
    
    [Serializable]
    public class PlayableTrack //A "level
    {
        public List<Beat> Beats;

        private TrackDefinition MainDefinition = null;
        
         public TrackDefinition ActiveDefinition;
        
        public World World = null;

        public bool IsValid = true;
        public bool Passed = false;
        
        
       public readonly int Index;
        
        public PlayableTrack(TrackDefinition definition, World world, int index)
        { 
            MainDefinition = definition;
            Index = index;
            World = world;
        }

        public void Init()
        {
            SM.Instance<EventManager>().RegisterListener<TrackFailed>(OnTrackFailed);
            SM.Instance<EventManager>().RegisterListener<TrackPassed>(OnTrackPassed);
            SM.Instance<EventManager>().RegisterListener<TrackResetEvent>(OnTrackReset);
            
            SetMain();
        }

        public void Kill()
        {
            SM.Instance<EventManager>().UnregisterListener<TrackFailed>(OnTrackFailed);
            SM.Instance<EventManager>().UnregisterListener<TrackPassed>(OnTrackPassed);
            SM.Instance<EventManager>().UnregisterListener<TrackResetEvent>(OnTrackReset);
        }

        private void SetMain()
        {
            IsValid = true;
            Passed = false;
            
            ActiveDefinition = MainDefinition;
            
            //What percentage of the track are we reserving as active
            int activeBeats =Mathf.CeilToInt( ActiveDefinition.BeatResolution * ActiveDefinition.ActiveBeatRange);
           
            //Remaining dead space
            int deadBeats = ActiveDefinition.BeatResolution - activeBeats;

            //Create a base set of beats up to the required total
            List<Beat> AllBeats = Enumerable.Range(0, ActiveDefinition.BeatResolution).Select(i => new Beat(BeatAction.Empty,i* (1f/ActiveDefinition.BeatResolution),0.001f,Beat.States.Upcoming,i )).ToList();
            
            //Take a normalised range from the middle
            //We add half deadzone to zero (since we want to shift it to the middle)
            //This leaves half at the end
            //We take the amount of active beats in the middle
            var ActiveZoneBeats = AllBeats.Skip((deadBeats/2)).Take(activeBeats);

            //And get a pattern
            //How to get a random pattern
            var randomValue = Random.value;

            if (randomValue > 0.65f)
            {
                BeatPatternThreeOnOneOff(ActiveZoneBeats);      
            }
            else if (randomValue > 0.35f)
            {
                BeatPatternTwoOnOneOff(ActiveZoneBeats);     
            }
            else if (randomValue > 0.25f)
            {
                BeatPatternGeneric(ActiveZoneBeats,4,2);             
            }
            else
            {
                BeatPatternGeneric(ActiveZoneBeats,2,2);              
            }
            
            
            //Assign the transformed beats
            Beats = AllBeats;
        }

        private void SetLaunch()
        {
            //We convert the track to a launch track
            Debug.LogFormat($"The track was passed.");
          
            IsValid = false;
            Passed = true;
            
            //Rewrite the definition
            //Some way to clone definitions?
            TrackDefinition newDefinition = new TrackDefinition(id:Random.Range(10000, 99999), new List<BeatAction>(){BeatAction.Transfer}, 3f, 3f, 64, 0.5f, 0.05f);
            // newDefinition.BeatResolution = ActiveDefinition.BeatResolution;
            // newDefinition.ActiveBeatRange = 0.5f;
            // newDefinition.ActiveDuration = 3f;
            // newDefinition.DeadDuration = 3f;
            // //A bigger buffer
            // newDefinition.Buffer = 0.05f;

            ActiveDefinition = newDefinition;
            
            //What percentage of the track are we reserving as active
            int activeBeats =Mathf.CeilToInt( ActiveDefinition.BeatResolution * ActiveDefinition.ActiveBeatRange);
           
            //Remaining dead space
            int deadBeats = ActiveDefinition.BeatResolution - activeBeats;

            //Create a base set of beats up to the required total
            List<Beat> AllBeats = Enumerable.Range(0, ActiveDefinition.BeatResolution).Select(i => new Beat(BeatAction.Empty,i* (1f/ActiveDefinition.BeatResolution),0.001f,Beat.States.Upcoming,i )).ToList();
            
            //Take a normalised range from the middle
            //We add half deadzone to zero (since we want to shift it to the middle)
            //This leaves half at the end
            //We take the amount of active beats in the middle
            var ActiveZoneBeats = AllBeats.Skip((deadBeats/2)).Take(activeBeats);

            //And get a pattern
            //BeatPatternTwoOnOneOff(ActiveZoneBeats);
            BeatPatternMiddle(ActiveZoneBeats);
            
            //This should be a method like 'Apply definition
            //Reassign the playable track beats
            Beats = AllBeats; 
        }
        
        private void OnTrackPassed(TrackPassed trackPassed) 
        {
            SetLaunch();
        }
        
        private void OnTrackFailed(TrackFailed context)
        {
            IsValid = false;
        }

        private void OnTrackReset(TrackResetEvent trackResetEvent) 
        {
            IsValid = true;
            
            foreach (var beat in Beats)
            {
                beat.SetState(Beat.States.Upcoming);
            }
        }
        
        //Select Only a middle beat
        public void BeatPatternMiddle(IEnumerable<Beat> source)
        {
            for (int i = 0; i < source.Count(); i++)
            {
                if (i == source.Count()/2)
                {
                    source.ElementAt(i).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                }
                else
                {
                    source.ElementAt(i).Action = BeatAction.Empty;
                }
            }
        }

        //Take a list of beats and make every other active
        public void BeatPatternEveryOther(IEnumerable<Beat> source)
        {
            for (int i = 0; i < source.Count(); i++)
            {
                if (i % 2 == 0)
                {
                    source.ElementAt(i).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                }
                else
                {
                    source.ElementAt(i).Action = BeatAction.Empty;
                }
            }
        }
        
        //Return a pattern of 00-x-00-x-00-x ...
        public void BeatPatternTwoOnOneOff(IEnumerable<Beat> source)
        {
             for (int i= 0; i<source.Count()-2 ;i +=3)
             {
                 source.ElementAt(i).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                 source.ElementAt(i+1).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
             }
        }
        
        //Return a pattern of 00-x-00-x-00-x ...
        public void BeatPatternThreeOnOneOff(IEnumerable<Beat> source)
        {
            for (int i= 0; i<source.Count()-3 ;i +=4)
            {
                source.ElementAt(i).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                source.ElementAt(i+1).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                source.ElementAt(i+2).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
            }
        }

       
        //Take a list of beats and make every other active
        public void BeatPatternGeneric(IEnumerable<Beat> source, int on, int off)
        {
            for (int i= 0; i<source.Count()-on ;i +=on+off)
            {
                for (int j = 0; j < on; j++)
                {
                    source.ElementAt(i+j).Action = (BeatAction)ActiveDefinition.ValidActions[Random.Range(0, ActiveDefinition.ValidActions.Count)];
                }
            }
        }
        
        //Take a list of beats and make every other active
        public void BeatPatternAllEmpty(List<Beat> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                    source[i].Action = BeatAction.Empty;
            }
        }
        // public void SetState(States state)
        // {
        //     //State = state;
        // }

        //Calculate a speed factor, that knows how fast the track should be playing
        // public float GetSpeed()
        // {
        //     return 1;
        // }
        
        public bool InActiveSpace(float normalisedProgress, float buffer)
        {
            var activeRemainder = 1 - ActiveDefinition.ActiveBeatRange;

            bool inActiveSpace = normalisedProgress > (activeRemainder / 2)-buffer && normalisedProgress < activeRemainder/2 + ActiveDefinition.ActiveBeatRange+buffer; 
  
            return inActiveSpace;
        }
        
        bool IsInFront(float A, float B) 
        {
            float clockwiseDistance = (B - A + 1) % 1;
            return clockwiseDistance < 0.5;              
        }
        
        public Beat? GetCurrentBeat(float fromT)
        {
            //Default to the first beat (always in front)
            //var closestBeat = Beats[0];
            var shortestDistance = float.MaxValue;
            
            //Then step forward
            //Send out a normalised ray at nominally small steps
            for(float n = 0; n<1f; n+=0.01f)
            {
                float newCheckN = Mathf.Repeat( fromT+n,1);
                
                foreach (var beat in Beats)
                {
                    if (beat.Action == BeatAction.Empty)
                    {
                        continue;
                    }
                
                    //Debug.DrawRay(OrbitHelpers.OrbitPointFromNormalisedPosition(World.Orbit,newCheckN), Vector3.up, Color.white,0.1f);
                    //Debug.DrawRay(OrbitHelpers.OrbitPointFromNormalisedPosition(World.Orbit,beat.NormalisedTime), Vector3.up,Color.white,0.1f);
                    
                    //Distance between this beat and the checking time
                    var distance = Mathf.Abs((beat.NormalisedTime) - newCheckN );

                    if (distance < ActiveDefinition.Buffer)
                    {
                        return beat;
                    }
                }
            }

            return null;
        }
        
        public Dictionary<int, float> NormalisedBeatTimes()
        {
            var normalizedBeatTimes = new Dictionary<int, float>();

            
            for(int i = 0; i<Beats.Count;i++)
            {
                normalizedBeatTimes.Add(i, (Beats[i].NormalisedTime));
            }

            return normalizedBeatTimes;
        }
    }
}