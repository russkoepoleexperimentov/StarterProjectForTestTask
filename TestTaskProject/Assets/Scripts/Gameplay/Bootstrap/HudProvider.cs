using static EventsProvider;
using UnityEngine;
using Zenject;

namespace Gameplay.Bootstrap
{
    public class HudProvider : MonoBehaviour
    {
        private const string HUD_SCREEN_ID = "HUD";

        private EventManager _eventManager;
        private LoadingState _loadingState;

        [Inject]
        private void Setup(EventManager eventManager, LoadingState loadingState)
        {
            _eventManager = eventManager;
            _loadingState = loadingState;
        }

        private void Start()
        {
            // сцена активируется до того, как загрузочный экран доиграет завершение,
            // поэтому HUD открываем только после того, как он освободит UIController
            if (_loadingState.IsRunning)
            {
                _eventManager.Subscribe<LoadingFinishedEvent>(HandleLoadingFinished);
                return;
            }

            OpenHud();
        }

        private void OnDestroy()
        {
            if (_eventManager != null)
                _eventManager.Unsubscribe<LoadingFinishedEvent>(HandleLoadingFinished);
        }

        private void HandleLoadingFinished(LoadingFinishedEvent finishedEvent)
        {
            _eventManager.Unsubscribe<LoadingFinishedEvent>(HandleLoadingFinished);
            OpenHud();
        }

        private void OpenHud() => _eventManager.Publish(new OpenScreenEvent(HUD_SCREEN_ID));
    }
}
