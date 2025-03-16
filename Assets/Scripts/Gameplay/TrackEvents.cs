using Events;
using Gameplay;

namespace TrackEvents
{
    public class TrackStarted : IEvent
    {
        public PlayableTrack Track { get; }

        public TrackStarted(PlayableTrack track)
        {
            Track = track;
        }
    }

    public class TrackPassed : IEvent
    {
        public PlayableTrack Track { get; }

        public TrackPassed(PlayableTrack track)
        {
            Track = track;
        }
    }

    public class TrackFailed : IEvent
    {
        public PlayableTrack Track { get; }

        public TrackFailed(PlayableTrack track)
        {
            Track = track;
        }
    }
    
    public class TransitionStarted : IEvent
    {
    }

    public class TransitionEnded : IEvent
    {
    }
}