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
        private readonly CarDamageModel _damage;

        public PlayerCarRegistrar(PlayerCarRegistry registry, CarStateModel state, CarView view,
            CarDamageModel damage)
        {
            _registry = registry;
            _state = state;
            _view = view;
            _damage = damage;
        }

        public void Initialize() => _registry.Set(_state, _view, _damage);

        public void Dispose() => _registry.Clear(_state);
    }
}
