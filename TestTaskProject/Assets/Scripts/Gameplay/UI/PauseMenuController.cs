using static EventsProvider;

namespace Gameplay.UI
{
    public class PauseMenuController : ScreenController
    {
        private readonly PauseMenuView _pauseView;

        public PauseMenuController(PauseMenuView view, EventManager eventManager) : base(view, eventManager)
        {
            _pauseView = view;
        }

        public override void Open()
        {
            base.Open();

            _pauseView.ResumeButton.onClick.AddListener(HandleResumeClicked);
            _pauseView.SaveAndExitButton.onClick.AddListener(HandleSaveAndExitClicked);
        }

        public override void Dispose()
        {
            if (_pauseView != null)
            {
                _pauseView.ResumeButton.onClick.RemoveListener(HandleResumeClicked);
                _pauseView.SaveAndExitButton.onClick.RemoveListener(HandleSaveAndExitClicked);
            }

            base.Dispose();
        }

        private void HandleResumeClicked() => _eventManager.Publish(new ResumeGameEvent());

        private void HandleSaveAndExitClicked() => _eventManager.Publish(new SaveAndExitEvent());
    }
}
