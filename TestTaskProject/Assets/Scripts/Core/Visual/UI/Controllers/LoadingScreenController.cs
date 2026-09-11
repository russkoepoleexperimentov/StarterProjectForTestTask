public class LoadingScreenController : ScreenController
{
    private readonly LoadingView _loadingView;
    private readonly LoadingState _state;

    private bool _completionStarted;

    public LoadingScreenController(LoadingView view, EventManager eventManager, LoadingState state)
        : base(view, eventManager)
    {
        _loadingView = view;
        _state = state;
    }

    public override void Open()
    {
        base.Open();

        _state.Changed += HandleStateChanged;
        _loadingView.SetOpaque();
        Render();
    }

    public override void Dispose()
    {
        _state.Changed -= HandleStateChanged;
        _state.CompletePresentation();
        base.Dispose();
    }

    private void HandleStateChanged() => Render();

    private void Render()
    {
        _loadingView.SetDescription(_state.Description);
        _loadingView.SetProgress(_state.Progress);

        if (!_state.IsCompleted || _completionStarted) return;

        _completionStarted = true;
        _loadingView.PlayCompletion(_state.CompletePresentation);
    }
}
