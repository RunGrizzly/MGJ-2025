
namespace Gameplay
{
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