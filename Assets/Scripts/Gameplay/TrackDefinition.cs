#nullable enable
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Gameplay
{
    //Make this a scriptable object?
    [Serializable]
    public class TrackDefinition
    {
        public int ID = 99999;
        public List<BeatAction> Actions { get; set; } = new();

        public float ActiveDuration;
        
        public float DeadDuration;

        public int BeatResolution;
        
        [UnityEngine.Range(0,1)]
        public float ActiveBeatRange;

        //A buffer (in normalised time) that inputs will be given leeway within
        [UnityEngine.Range(0.001f,0.02f)]
        public float Buffer;

        public List<BeatAction> ValidActions = new List<BeatAction>();

        public TrackDefinition(int id, List<BeatAction> validActions, float activeDuration = 5f, float deadDuration= 5f, int beatresolution = 64, float activeBeatRange = 0.25f, float buffer = 0.01f)
        {
            ID = id;
            ValidActions = validActions;
            ActiveDuration = activeDuration;
            DeadDuration = deadDuration;
            BeatResolution = beatresolution;
            ActiveBeatRange = activeBeatRange;
            Buffer = buffer;
        }
    }
}