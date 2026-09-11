using UnityEngine;
using Zenject;
using static EventsProvider;

public class MainMenuProvider : MonoBehaviour
{
    private EventManager _eventManager;

    private const string MAIN_MENU_VIEW = "MainMenu";

    [Inject]
    private void Setup(EventManager eventManager) 
    {
        _eventManager = eventManager;
    }

    // при загрузке сцены стартуем главное меню
    private void Start()
    {
        _eventManager.Publish(new OpenScreenEvent("MainMenu"));
    }
}
