using Gameplay.SaveLoad;
using static EventsProvider;

namespace Gameplay.UI
{
    public class MainMenuController : ScreenController
    {
        private readonly MainMenuView _menuView;
        private readonly GameSaveService _saveService;

        public MainMenuController(MainMenuView view, EventManager eventManager, GameSaveService saveService)
            : base(view, eventManager)
        {
            _menuView = view;
            _saveService = saveService;
        }

        public override void Open()
        {
            base.Open();

            foreach (var button in _menuView.LoadSceneButtons)
            {
                button.Clicked += HandleLoadSceneClicked;

                if (button.ContinuesFromSave)
                    button.SetInteractable(_saveService.HasSave);
            }
        }

        public override void Dispose()
        {
            foreach (var button in _menuView.LoadSceneButtons)
                button.Clicked -= HandleLoadSceneClicked;

            base.Dispose();
        }

        private void HandleLoadSceneClicked(LoadSceneButton button)
        {
            _saveService.ContinueRequested = button.ContinuesFromSave;

            _eventManager.Publish(new StartLoadingEvent(
                new SceneLoadingOperation(button.SceneName, button.LoadingDescription)));
        }
    }
}
