using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static EventsProvider;

public class LoadingService
{
    private readonly EventManager _eventManager;
    private readonly UIController _uiController;
    private readonly LoadingState _state;
    private readonly Queue<ILoadingOperation> _loadingOperations = new();

    private bool _active;

    private const int PRESENTATION_TIMEOUT_MS = 10000;
    private const string LOADING_SCREEN_ID = "LoadingScreen";

    public LoadingService(EventManager eventManager, UIController controller, LoadingState state)
    {
        _eventManager = eventManager;
        _uiController = controller;
        _state = state;
        _eventManager.Subscribe<StartLoadingEvent>(HandleStartLoading);
    }

    public void AppendOperation(ILoadingOperation operation)
    {
        if (operation == null) return;

        _loadingOperations.Enqueue(operation);
        LoadAll();
    }

    private void HandleStartLoading(StartLoadingEvent startEvent) => AppendOperation(startEvent.Operation);

    private async void LoadAll()
    {
        if (_active) return;

        _active = true;

        try
        {
            _state.Reset();
            _eventManager.Publish(new OpenScreenEvent(LOADING_SCREEN_ID));

            while (_loadingOperations.Count > 0)
            {
                var currentOperation = _loadingOperations.Dequeue();
                await currentOperation.Run(_state.SetProgress, _state.SetDescription);
            }

            _state.Complete();
            await WaitForPresentation();

            _uiController.Clear();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            _loadingOperations.Clear();
            _uiController.Clear();
        }
        finally
        {
            _active = false;
        }
    }

    private async Task WaitForPresentation()
    {
        var finished = await Task.WhenAny(_state.PresentationCompleted, Task.Delay(PRESENTATION_TIMEOUT_MS));

        if (finished != _state.PresentationCompleted)
            Debug.LogWarning($"Loading screen presentation did not finish in {PRESENTATION_TIMEOUT_MS} ms.");
    }
}
