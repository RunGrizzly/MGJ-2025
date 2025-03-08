using System.Collections.Generic;
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

    public class LevelPassed : IEvent
    {
        public Level Level { get; }

        public LevelPassed(Level level)
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
            ExitTrack = GetExitTrack(track);
            Number = number;
        }

        private static PlayableTrack GetExitTrack(PlayableTrack track)
        {
            var exitTime = 0.75f * track.Duration;
            var exitTrack = new PlayableTrack(new List<Beat>()
            {
                new(BeatAction.Transfer, exitTime, exitTime + 0.25f, Beat.States.Upcoming, 0)
            }, track.Duration);

            return exitTrack;
        }

        public World World { get; private set; }
        public PlayableTrack Track { get; private set; }
        public PlayableTrack ExitTrack { get; private set; }
        public int Number { get; private set; }
    }
}