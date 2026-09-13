using static EventsProvider;

namespace Gameplay.UI
{
    public class MainMenuController : ScreenController
    {
        private readonly MainMenuView _menuView;

        public MainMenuController(MainMenuView view, EventManager eventManager) : base(view, eventManager)
        {
            _menuView = view;
        }

        public override void Open()
        {
            base.Open();

            foreach (var button in _menuView.LoadSceneButtons)
                button.Clicked += HandleLoadSceneClicked;
        }

        public override void Dispose()
        {
            foreach (var button in _menuView.LoadSceneButtons)
                button.Clicked -= HandleLoadSceneClicked;

            base.Dispose();
        }

        private void HandleLoadSceneClicked(LoadSceneButton button)
        {
            _eventManager.Publish(new StartLoadingEvent(
                new SceneLoadingOperation(button.SceneName, button.LoadingDescription)));
        }
    }
}
