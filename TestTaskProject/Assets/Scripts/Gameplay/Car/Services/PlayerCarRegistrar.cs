using System;
using Gameplay.Car.Model;
using Gameplay.Car.View;
using Zenject;

namespace Gameplay.Car.Services
{
    public class PlayerCarRegistrar : IInitializable, IDisposable
    {
        private readonly PlayerCarRegistry _registry;
        private readonly CarStateModel _state;
        private readonly CarView _view;

        public PlayerCarRegistrar(PlayerCarRegistry registry, CarStateModel state, CarView view)
        {
            _registry = registry;
            _state = state;
            _view = view;
        }

        public void Initialize() => _registry.Set(_state, _view);

        public void Dispose() => _registry.Clear(_state);
    }
}
