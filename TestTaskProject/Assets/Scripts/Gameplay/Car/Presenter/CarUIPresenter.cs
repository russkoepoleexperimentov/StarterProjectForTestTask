using System;
using Gameplay.Car.Model;
using Gameplay.Car.Services;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarUIPresenter : IInitializable, IDisposable
    {
        private readonly CarUIView _view;
        private readonly PlayerCarRegistry _registry;

        private CarStateModel _state;

        private const string NEUTRAL_GEAR_LABEL = "N";
        private const string REVERSE_GEAR_LABEL = "R";

        public CarUIPresenter(CarUIView view, PlayerCarRegistry registry)
        {
            _view = view;
            _registry = registry;
        }

        public void Initialize()
        {
            _registry.Changed += HandlePlayerCarChanged;
            HandlePlayerCarChanged(_registry.Current);
        }

        public void Dispose()
        {
            _registry.Changed -= HandlePlayerCarChanged;
            Unbind();
        }

        private void HandlePlayerCarChanged(CarStateModel state)
        {
            Unbind();

            _state = state;

            if (_state == null)
            {
                _view.SetVisible(false);
                return;
            }

            _state.Changed += Render;
            _view.SetVisible(true);
            Render(_state);
        }

        private void Unbind()
        {
            if (_state == null) return;

            _state.Changed -= Render;
            _state = null;
        }

        private void Render(CarStateModel state)
        {
            _view.SetGear(FormatGear(state.GearIndex));
            _view.SetSpeed(Mathf.RoundToInt(Mathf.Abs(state.SpeedKph)));
            _view.SetRpm(Mathf.RoundToInt(state.RPM));
            _view.SetInput(state.Throttle, state.Brake, state.Steering, state.ClutchEngagement);
        }

        private static string FormatGear(int gearIndex)
        {
            if (gearIndex == 0) return NEUTRAL_GEAR_LABEL;
            if (gearIndex < 0) return REVERSE_GEAR_LABEL;

            return gearIndex.ToString();
        }
    }
}
