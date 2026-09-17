using System;
using System.Threading.Tasks;

public class LoadingState
{
    public float Progress { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public bool IsRunning { get; private set; }
    public Task PresentationCompleted => _presentationCompletion.Task;

    public event Action Changed;

    private TaskCompletionSource<bool> _presentationCompletion = CreateCompletionSource();

    public void Reset()
    {
        IsRunning = true;
        Progress = 0;
        Description = string.Empty;
        IsCompleted = false;
        _presentationCompletion = CreateCompletionSource();
        Changed?.Invoke();
    }

    public void SetProgress(float progress)
    {
        var clamped = progress < 0 ? 0 : progress > 1 ? 1 : progress;

        if (Progress == clamped) return;

        Progress = clamped;
        Changed?.Invoke();
    }

    public void SetDescription(string description)
    {
        if (Description == description) return;

        Description = description;
        Changed?.Invoke();
    }

    public void Complete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        Changed?.Invoke();
    }

    public void FinishRunning()
    {
        IsRunning = false;
    }

    public void CompletePresentation()
    {
        _presentationCompletion.TrySetResult(true);
    }

    private static TaskCompletionSource<bool> CreateCompletionSource()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
