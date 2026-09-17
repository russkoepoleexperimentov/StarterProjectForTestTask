using static EventsProvider;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Gameplay.Bootstrap
{
    public class PauseProvider : MonoBehaviour
    {
        public const string PAUSE_SCREEN_ID = "PauseMenu";

        private EventManager _eventManager;
        private LoadingState _loadingState;

        private bool _paused;

        [Inject]
        private void Setup(EventManager eventManager, LoadingState loadingState)
        {
            _eventManager = eventManager;
            _loadingState = loadingState;
        }

        private void Start()
        {
            _eventManager.Subscribe<ResumeGameEvent>(HandleResume);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;

            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (_loadingState.IsRunning) return;

            if (_paused)
                _eventManager.Publish(new ResumeGameEvent());
            else
                Pause();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;

            if (_eventManager != null)
                _eventManager.Unsubscribe<ResumeGameEvent>(HandleResume);
        }

        private void Pause()
        {
            _paused = true;
            Time.timeScale = 0;
            _eventManager.Publish(new OpenScreenEvent(PAUSE_SCREEN_ID));
        }

        private void HandleResume(ResumeGameEvent resumeEvent)
        {
            if (!_paused) return;

            _paused = false;
            Time.timeScale = 1;
        }
    }
}
