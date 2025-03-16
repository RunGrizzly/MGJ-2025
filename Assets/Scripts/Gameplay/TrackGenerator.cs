using System.Collections.Generic;
using System.Linq;
using Gameplay;
using NUnit.Framework;
using UnityEngine;


    public class TrackGenerator : MonoBehaviour
    {
        public Vector2 ActiveDurationGamut = new Vector2(12, 5);
        public Vector2 DeadDurationGamut = new Vector2(12, 3);
        public Vector2Int BeatResolutionGamut = new Vector2Int(32, 256);
        public Vector2 ActiveBeatRangeGamut = new Vector2(0.25f, 0.5f);
        
        // public TrackDefinition GetRandomTrackDefinition(Vector2 activeDurationGamut, Vector2 deadDurationGamut, Vector2Int beatResolutionGamut, Vector2 activeBeatRangeGamut)
        // {
        //     //Random values
        //     var activeDuration = Random.Range(activeDurationGamut.x, activeDurationGamut.y);
        //     var deadDuration = Random.Range(deadDurationGamut.x, deadDurationGamut.y);
        //     var beatResolution = Random.Range(beatResolutionGamut.x, beatResolutionGamut.y);
        //     var activeBeatRange = Random.Range(activeBeatRangeGamut.x, activeBeatRangeGamut.y);
        //     //
        //     return new TrackDefinition(id:Random.Range(10000, 99999),activeDuration,deadDuration,beatResolution,activeBeatRange);
        // }
        
        public TrackDefinition GetRandomTrackDefinition(float difficultyFactor)
        {
            //Random values
            var activeDuration = QuantisedLerp(ActiveDurationGamut.x, ActiveDurationGamut.y,difficultyFactor, 0.5f);
            var deadDuration = QuantisedLerp(DeadDurationGamut.x, DeadDurationGamut.y,difficultyFactor, 0.5f);
            var beatResolution = QuantisedLerp(BeatResolutionGamut.x, BeatResolutionGamut.y,difficultyFactor, 8);
            var activeBeatRange = QuantisedLerp(ActiveBeatRangeGamut.x, ActiveBeatRangeGamut.y,difficultyFactor,0.05f);
            
            var actionLimit = QuantisedLerp(1, 4, difficultyFactor*5f, 1);

            List<BeatAction> ValidActions = Enumerable.Range(1, (int)actionLimit).Select(i =>  (BeatAction)i ).ToList();
            
            
            return new TrackDefinition(id:Random.Range(10000, 99999),ValidActions, activeDuration,deadDuration,(int)beatResolution,activeBeatRange);
        }
        
        float QuantisedLerp(float minValue, float maxValue,float t, float quantiseStep)
        {
            // Standard lerp
            float rawValue = Mathf.Lerp(minValue, maxValue, t);
            
            // Quantize to nearest multiple of stepSize
            return Mathf.Round(rawValue / quantiseStep) * quantiseStep;
        }
    }