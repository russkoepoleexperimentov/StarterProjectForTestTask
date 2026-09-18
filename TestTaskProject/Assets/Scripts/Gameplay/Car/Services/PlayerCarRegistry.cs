using System;
using Gameplay.Car.Model;
using Gameplay.Car.View;

namespace Gameplay.Car.Services
{
    public class PlayerCarRegistry
    {
        public CarStateModel Current { get; private set; }
        public CarView CurrentView { get; private set; }
        public CarDamageModel CurrentDamage { get; private set; }

        public event Action<CarStateModel> Changed;

        public void Set(CarStateModel state, CarView view, CarDamageModel damage)
        {
            if (Current == state) return;

            Current = state;
            CurrentView = view;
            CurrentDamage = damage;
            Changed?.Invoke(Current);
        }

        public void Clear(CarStateModel state)
        {
            if (Current != state) return;

            Current = null;
            CurrentView = null;
            CurrentDamage = null;
            Changed?.Invoke(null);
        }
    }
}
