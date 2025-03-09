#nullable enable
using System.Collections.Generic;

namespace Gameplay
{
    public class TrackDefinition
    {
        public List<BeatAction> Actions { get; set; } = new();
    }
}