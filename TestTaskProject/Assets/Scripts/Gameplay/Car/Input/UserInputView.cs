using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.Input
{
    public class UserInputView : CarInputHandler
    {
        private PlayerInput _input;

        private void Awake() {
            _input = new();
            _input.Enable();
        }

        private void OnDestroy() {
            if (_input == null) return;

            _input.Disable();
            _input.Dispose();
            _input = null;
        }

        protected override DrivetrainInputModel ReadRaw() {
            if (_input == null)
                return DrivetrainInputModel.Idle;

            var input = _input.Game.Movement.ReadValue<Vector2>();
            var throttle = Mathf.Clamp01(input.y);
            var brake = Mathf.Clamp01(-input.y);
            var steering = input.x;
            var handbrake = Mathf.Clamp01(_input.Game.Handbrake.ReadValue<float>());
            var clutch = Mathf.Clamp01(_input.Game.Clutch.ReadValue<float>());

            return DrivetrainInputModel.Raw(throttle, brake, steering, clutch, handbrake);
        }
    }
}
