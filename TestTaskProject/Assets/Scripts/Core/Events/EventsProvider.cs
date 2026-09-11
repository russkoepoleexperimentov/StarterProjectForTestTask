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

    public struct UpdateLoadingContextEvent 
    {
        public readonly string Description;

        public UpdateLoadingContextEvent(string description)
        {
            Description = description;
        }
    }

    public struct UpdateLoadingProgressEvent 
    {
        public readonly float Progress;

        public UpdateLoadingProgressEvent(float progress)
        {
            Progress = progress;
        }
    }

    public struct StartLoadingScreenFadeEvent 
    {
        public readonly float TimeMs;

        public StartLoadingScreenFadeEvent(float timeMs)
        {
            TimeMs = timeMs;
        }
    }
}
