using UnityEngine;

namespace Gameplay
{
    public class Level : MonoBehaviour
    {
        public World World { get; private set; }
        public PlayableTrack Track { get; private set; }
        public int Number { get; private set; }
    }
}