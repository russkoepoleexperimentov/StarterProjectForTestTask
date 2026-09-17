using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class PauseMenuView : ScreenView
    {
        public Button ResumeButton => _resumeButton;
        public Button SaveAndExitButton => _saveAndExitButton;

        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _saveAndExitButton;

        public override ScreenController Construct(EventManager eventManager)
        {
            return new PauseMenuController(this, eventManager);
        }
    }
}
