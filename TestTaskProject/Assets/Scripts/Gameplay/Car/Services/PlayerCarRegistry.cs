using System;
using Gameplay.Car.Model;
using Gameplay.Car.View;

namespace Gameplay.Car.Services
{
    public class PlayerCarRegistry
    {
        public CarStateModel Current { get; private set; }
        public CarView CurrentView { get; private set; }

        public event Action<CarStateModel> Changed;

        public void Set(CarStateModel state, CarView view)
        {
            if (Current == state) return;

            Current = state;
            CurrentView = view;
            Changed?.Invoke(Current);
        }

        public void Clear(CarStateModel state)
        {
            if (Current != state) return;

            Current = null;
            CurrentView = null;
            Changed?.Invoke(null);
        }
    }
}
