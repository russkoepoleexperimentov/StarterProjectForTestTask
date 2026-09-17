using Gameplay.Car.Model;
using Gameplay.Car.Services;
using Gameplay.Car.View;
using UnityEngine;

namespace Gameplay.Car.Presenter
{
    public class CarUIScreenController : ScreenController
    {
        private readonly CarUIView _carView;
        private readonly PlayerCarRegistry _registry;

        private CarStateModel _state;

        private const string NEUTRAL_GEAR_LABEL = "N";
        private const string REVERSE_GEAR_LABEL = "R";

        public CarUIScreenController(CarUIView view, EventManager eventManager, PlayerCarRegistry registry)
            : base(view, eventManager)
        {
            _carView = view;
            _registry = registry;
        }

        public override void Open()
        {
            base.Open();

            _registry.Changed += HandlePlayerCarChanged;
            HandlePlayerCarChanged(_registry.Current);
        }

        public override void Dispose()
        {
            _registry.Changed -= HandlePlayerCarChanged;
            Unbind();
            base.Dispose();
        }

        private void HandlePlayerCarChanged(CarStateModel state)
        {
            Unbind();

            _state = state;

            if (_state == null)
            {
                _carView.SetVisible(false);
                return;
            }

            _state.Changed += Render;
            _carView.SetVisible(true);
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
            _carView.SetGear(FormatGear(state.GearIndex));
            _carView.SetSpeed(Mathf.RoundToInt(Mathf.Abs(state.SpeedKph)));
            _carView.SetRpm(Mathf.RoundToInt(state.RPM));
            _carView.SetInput(state.Throttle, state.Brake, state.Steering, state.ClutchEngagement);
        }

        private static string FormatGear(int gearIndex)
        {
            if (gearIndex == 0) return NEUTRAL_GEAR_LABEL;
            if (gearIndex < 0) return REVERSE_GEAR_LABEL;

            return gearIndex.ToString();
        }
    }
}
