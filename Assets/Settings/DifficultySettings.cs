using UnityEngine;

namespace Settings
{
    [CreateAssetMenu]
    public class DifficultySettings : ScriptableObject
    {
        public float BeatRate;
        public float BeatTimingWindow;
    }
}