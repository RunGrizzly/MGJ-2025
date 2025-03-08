using Events;

namespace Gameplay
{
    public class NewLevel : IEvent
    {
        public Level Level { get; }

        public NewLevel(Level level)
        {
            Level = level;
        }
    }
    
    public class Level 
    {
        public Level(World world, PlayableTrack track, int number)
        {
            World = world;
            Track = track;
            Number = number;
        }

        public World World { get; private set; }
        public PlayableTrack Track { get; private set; }
        public int Number { get; private set; }
    }
}

