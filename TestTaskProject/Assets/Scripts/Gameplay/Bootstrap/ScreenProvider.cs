using static EventsProvider;
using UnityEngine;
using Zenject;

namespace Gameplay.Bootstrap
{
    /// <summary>
    /// Открывает свой экран при старте сцены, дождавшись окончания загрузки:
    /// сцена активируется раньше, чем загрузочный экран доигрывает завершение.
    /// </summary>
    public abstract class ScreenProvider : MonoBehaviour
    {
        protected abstract string ScreenId { get; }

        protected EventManager EventManager { get; private set; }

        private LoadingState _loadingState;

        [Inject]
        private void Setup(EventManager eventManager, LoadingState loadingState)
        {
            EventManager = eventManager;
            _loadingState = loadingState;
        }

        protected virtual void Start()
        {
            if (_loadingState.IsRunning)
            {
                EventManager.Subscribe<LoadingFinishedEvent>(HandleLoadingFinished);
                return;
            }

            OpenScreen();
        }

        protected virtual void OnDestroy()
        {
            if (EventManager != null)
                EventManager.Unsubscribe<LoadingFinishedEvent>(HandleLoadingFinished);
        }

        protected void OpenScreen() => EventManager.Publish(new OpenScreenEvent(ScreenId));

        private void HandleLoadingFinished(LoadingFinishedEvent finishedEvent)
        {
            EventManager.Unsubscribe<LoadingFinishedEvent>(HandleLoadingFinished);
            OpenScreen();
        }
    }
}
