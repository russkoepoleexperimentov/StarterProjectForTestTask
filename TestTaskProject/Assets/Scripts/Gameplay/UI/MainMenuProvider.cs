using UnityEngine;
using Zenject;
using static EventsProvider;

public class MainMenuProvider : MonoBehaviour
{
    private const string MAIN_MENU_SCREEN_ID = "MainMenu";

    private EventManager _eventManager;

    [Inject]
    private void Setup(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    private void Start()
    {
        _eventManager.Publish(new OpenScreenEvent(MAIN_MENU_SCREEN_ID));
    }
}
