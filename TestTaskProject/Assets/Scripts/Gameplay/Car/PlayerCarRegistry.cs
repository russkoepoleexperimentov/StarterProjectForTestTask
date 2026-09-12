using System;

public class PlayerCarRegistry
{
    public CarStateModel Current { get; private set; }

    public event Action<CarStateModel> Changed;

    public void Set(CarStateModel state)
    {
        if (Current == state) return;

        Current = state;
        Changed?.Invoke(Current);
    }

    public void Clear(CarStateModel state)
    {
        if (Current != state) return;

        Current = null;
        Changed?.Invoke(null);
    }
}
