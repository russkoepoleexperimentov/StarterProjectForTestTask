using static EventsProvider;
using Gameplay.Car.Model;
using Gameplay.Car.Services;
using UnityEngine;
using Zenject;

namespace Gameplay.SaveLoad
{
    public class GameSaveProvider : MonoBehaviour
    {
        [SerializeField] private string _menuSceneName = "FirstScene";
        [SerializeField] private string _exitDescription = "Сохранение";

        private EventManager _eventManager;
        private GameSaveService _saveService;
        private PlayerCarRegistry _registry;

        [Inject]
        private void Setup(EventManager eventManager, GameSaveService saveService, PlayerCarRegistry registry)
        {
            _eventManager = eventManager;
            _saveService = saveService;
            _registry = registry;
        }

        private void Start()
        {
            _eventManager.Subscribe<SaveAndExitEvent>(HandleSaveAndExit);

            _registry.Changed += HandlePlayerCarChanged;
            HandlePlayerCarChanged(_registry.Current);
        }

        private void OnDestroy()
        {
            _registry.Changed -= HandlePlayerCarChanged;

            if (_eventManager != null)
                _eventManager.Unsubscribe<SaveAndExitEvent>(HandleSaveAndExit);
        }

        private void HandlePlayerCarChanged(CarStateModel state)
        {
            if (state == null || !_saveService.ContinueRequested) return;

            _saveService.ContinueRequested = false;

            if (!_saveService.TryLoad(out var data)) return;

            var view = _registry.CurrentView;

            if (view == null) return;

            view.Teleport(data.Position, data.Rotation);
        }

        private void HandleSaveAndExit(SaveAndExitEvent saveEvent)
        {
            var view = _registry.CurrentView;

            if (view != null)
                _saveService.Save(new CarSaveData(view.Position, view.Rotation));
            else
                Debug.LogWarning("Нет машины игрока — сохранять нечего.", this);

            Time.timeScale = 1;

            _eventManager.Publish(new StartLoadingEvent(
                new SceneLoadingOperation(_menuSceneName, _exitDescription)));
        }
    }
}
