namespace Events
{
    public class RunStaged : IEvent
    {
    }

    public class RunStarted : IEvent
    {
        public Run Run;

        public RunStarted(Run run)
        {
            Run = run;
        }
    }

    public class RunEnded : IEvent
    {
    }

    public class ResetGame : IEvent
    {
    }
}