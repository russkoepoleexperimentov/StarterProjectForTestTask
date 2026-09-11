using Zenject;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EventsProvider;

public class LoadingService 
{
    private readonly EventManager _eventManager;
    private readonly UIController _uiController;

    private Queue<ILoadingOperation> _loadingOperations;
    private bool _active;

    private const int DELAY_AFTER_LAST_LOADING_MS = 1000; 
    private const string LOADING_SCREEN_ID = "LoadingScreen";

    public LoadingService(EventManager eventManager, UIController controller) 
    {
        _eventManager = eventManager;
        _uiController = controller;
        _loadingOperations = new();
    }

    public void AppendOperation(ILoadingOperation operation) 
    {
        _loadingOperations.Enqueue(operation);
        LoadAll();
    }

    private async void LoadAll()
    {
        if(_active)
            return;

        _active = true;

        _eventManager.Publish(new OpenScreenEvent(LOADING_SCREEN_ID));

        while (_loadingOperations.Count > 0)
        {
            var currentOperation = _loadingOperations.Dequeue();
            
            await currentOperation.Run(UpdateProgressHandler, UpdateDescriptionHandler);
        }

        _eventManager.Publish(new StartLoadingScreenFadeEvent(DELAY_AFTER_LAST_LOADING_MS));
        await Task.Delay(DELAY_AFTER_LAST_LOADING_MS);

        _uiController.Clear(); // TODO: publish event "close screen LOADING_SCREEN_ID"
        _active = false;
    }

    private void UpdateDescriptionHandler(string description)
    {
        _eventManager.Publish(new UpdateLoadingContextEvent(description));
    }

    private void UpdateProgressHandler(float progress)
    {
        _eventManager.Publish(new UpdateLoadingProgressEvent(progress));
    }
}