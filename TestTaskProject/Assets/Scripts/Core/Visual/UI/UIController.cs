using UnityEngine;
using Zenject;
using static EventsProvider;

public class UIController : MonoBehaviour
{
    [SerializeField] private ScreenView[] _views = new ScreenView[0];

    private ScreenController _currentScreen;
    private ScreenView _currentView;
    private EventManager _eventManager;
    private DiContainer _container;

    [Inject]
    public void Initialize(EventManager eventManager, DiContainer container)
    {
        Unsubscribe();

        _eventManager = eventManager;
        _container = container;
        _eventManager.Subscribe<OpenScreenEvent>(OpenScreen);
        _eventManager.Subscribe<CloseScreenEvent>(CloseScreen);
    }

    private void OpenScreen(OpenScreenEvent screenEvent)
    {
        if (string.IsNullOrEmpty(screenEvent.ScreenId)) return;

        foreach (var view in _views)
        {
            if (view == null || view.Id != screenEvent.ScreenId) continue;
            if (_currentView != null && _currentView.Id == view.Id) return;

            Clear();
            _currentView = _container.InstantiatePrefabForComponent<ScreenView>(view, transform);
            _currentScreen = _currentView.Construct(_eventManager);
            _currentScreen.Open();
            return;
        }

        Debug.LogWarning($"Screen '{screenEvent.ScreenId}' is not registered.", this);
    }

    private void CloseScreen(CloseScreenEvent screenEvent)
    {
        if (_currentView == null || _currentView.Id != screenEvent.ScreenId) return;

        Clear();
    }

    private void Clear()
    {
        if (_currentScreen != null)
        {
            _currentScreen.Close();
            _currentScreen.Dispose();
        }

        if (_currentView != null)
            Destroy(_currentView.gameObject);

        _currentScreen = null;
        _currentView = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
        Clear();
    }

    private void Unsubscribe()
    {
        if (_eventManager == null) return;

        _eventManager.Unsubscribe<OpenScreenEvent>(OpenScreen);
        _eventManager.Unsubscribe<CloseScreenEvent>(CloseScreen);
    }
}
