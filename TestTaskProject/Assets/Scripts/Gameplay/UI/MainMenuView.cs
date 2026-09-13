using UnityEngine;

namespace Gameplay.UI
{
    public class MainMenuView : ScreenView
    {
        public LoadSceneButton[] LoadSceneButtons => _loadSceneButtons ??= GetComponentsInChildren<LoadSceneButton>(true);

        private LoadSceneButton[] _loadSceneButtons;

        public override ScreenController Construct(EventManager eventManager)
        {
            return new MainMenuController(this, eventManager);
        }
    }
}
