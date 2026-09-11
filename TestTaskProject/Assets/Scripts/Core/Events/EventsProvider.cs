public static class EventsProvider
{
    public class OpenScreenEvent
    {
        public readonly string ScreenId;

        public OpenScreenEvent(string screenId)
        {
            ScreenId = screenId;
        }
    }

    public struct StartLoadingEvent
    {
        public readonly ILoadingOperation Operation;

        public StartLoadingEvent(ILoadingOperation operation)
        {
            Operation = operation;
        }
    }
}
