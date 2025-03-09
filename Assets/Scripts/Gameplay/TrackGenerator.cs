using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class TrackGenerator : MonoBehaviour
    {
        private static readonly TrackDefinition EasyTrackDefinition = new()
        {
            Actions = { BeatAction.Action1, BeatAction.Empty, BeatAction.Action1, BeatAction.Empty }
        };

        private static readonly TrackDefinition HardTrackDefinition = new()
        {
            Actions = { BeatAction.Action1, BeatAction.Action2, BeatAction.Action4, BeatAction.Action3 }
        };

        public TrackDefinition Generate(int segmentCount, int possibleActions)
        {
            var actions = Enumerable.Range(0, 16).Select(_ => (BeatAction)Random.Range(0, possibleActions + 1));
            return new TrackDefinition() { Actions = actions.ToList() };
        }
    }
}