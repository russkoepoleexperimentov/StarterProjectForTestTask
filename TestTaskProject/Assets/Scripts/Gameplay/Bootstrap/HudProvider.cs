using static EventsProvider;

namespace Gameplay.Bootstrap
{
    public class HudProvider : ScreenProvider
    {
        public const string HUD_SCREEN_ID = "HUD";

        protected override string ScreenId => HUD_SCREEN_ID;

        protected override void Start()
        {
            base.Start();

            EventManager.Subscribe<ResumeGameEvent>(HandleResume);
        }

        protected override void OnDestroy()
        {
            if (EventManager != null)
                EventManager.Unsubscribe<ResumeGameEvent>(HandleResume);

            base.OnDestroy();
        }

        private void HandleResume(ResumeGameEvent resumeEvent) => OpenScreen();
    }
}
