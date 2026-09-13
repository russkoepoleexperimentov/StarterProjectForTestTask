using System;
using Gameplay.Car.Model;
using Zenject;

namespace Gameplay.Car.Services
{
    public class PlayerCarRegistrar : IInitializable, IDisposable
    {
        private readonly PlayerCarRegistry _registry;
        private readonly CarStateModel _state;

        public PlayerCarRegistrar(PlayerCarRegistry registry, CarStateModel state)
        {
            _registry = registry;
            _state = state;
        }

        public void Initialize() => _registry.Set(_state);

        public void Dispose() => _registry.Clear(_state);
    }
}
