using Gameplay.SaveLoad;
using UnityEngine;
using Zenject;

namespace Gameplay.UI
{
    public class MainMenuView : ScreenView
    {
        public LoadSceneButton[] LoadSceneButtons => _loadSceneButtons ??= GetComponentsInChildren<LoadSceneButton>(true);

        private LoadSceneButton[] _loadSceneButtons;

        [Inject] private GameSaveService _saveService;

        public override ScreenController Construct(EventManager eventManager)
        {
            return new MainMenuController(this, eventManager, _saveService);
        }
    }
}
