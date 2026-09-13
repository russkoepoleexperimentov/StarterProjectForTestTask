using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    // физика двигателя и трансмиссии. Всё, что относится к интерпретации ввода
    // (коррекция газа, пассивное торможение, автокоробка, педаль сцепления),
    // живёт в Gameplay.Car.Input и приезжает сюда готовым в DrivetrainInputModel.
    public class DrivetrainModel
    {
        private readonly EngineConfig _engineConfig;
        private readonly GearboxUsageConfig _gearboxUsageConfig;
        private readonly CarSystemsConfig _carSystemsConfig;
        private readonly GearboxModel _gearbox;
        private readonly EngineTorqueCurve _torqueCurve;
        private readonly CarStateModel _stateModel;

        private float _engineRpm;
        private float _velocity;

        private const float RPM_ADJUST_SMOOTHNESS = 3;

        public DrivetrainModel(
            EngineConfig engineConfig,
            GearboxUsageConfig gearboxUsageConfig,
            CarSystemsConfig carSystemsConfig,
            GearboxModel gearbox,
            EngineTorqueCurve torqueCurve,
            CarStateModel stateModel
            )
        {
            _engineConfig = engineConfig;
            _gearboxUsageConfig = gearboxUsageConfig;
            _carSystemsConfig = carSystemsConfig;
            _gearbox = gearbox;
            _torqueCurve = torqueCurve;
            _stateModel = stateModel;

            _engineRpm = _engineConfig.IdleRPM;
        }

        public DrivetrainOutputModel Tick(DrivetrainInputModel input, float averageWheelsRpm, float speedKph, float deltaTime)
        {
            var producedTorque = CalculateEngine(input, averageWheelsRpm, deltaTime, out var engineBrakeTorque);

            var brakeTorque = input.Brake * _carSystemsConfig.MaxBrakeTorque;
            var handBrakeTorque = input.Handbrake * _carSystemsConfig.MaxHandBrakeTorque;
            var steerAngle = input.Steering * _carSystemsConfig.MaxSteerAngle;

            _stateModel.Set(_engineRpm, speedKph, _gearbox.CurrentGear,
                input.Throttle, input.Brake, input.Steering, input.Handbrake, input.ClutchEngagement);

            return new DrivetrainOutputModel(producedTorque, brakeTorque, handBrakeTorque, engineBrakeTorque, steerAngle);
        }

        private float CalculateEngine(DrivetrainInputModel input, float averageWheelsRpm, float deltaTime,
            out float engineBrakeTorque)
        {
            var throttle = input.ThrottleCut ? 0f : input.Throttle;

            var grossTorque = _torqueCurve.Evaluate(_engineRpm) * throttle;
            var frictionTorque = _torqueCurve.EvaluateFriction(_engineRpm, throttle);

            var engineTorque = grossTorque - frictionTorque;

            var freeRpm = _engineRpm + engineTorque * deltaTime / _engineConfig.InertiaKgM;

            var ratio = _gearbox.CurrentRatio;
            var clutch = input.ClutchEngagement;

            var drivingTorque = Mathf.Max(engineTorque, 0f);
            var brakingTorque = Mathf.Max(-engineTorque, 0f) * _torqueCurve.EvaluateEngineBrakeFade(_engineRpm);

            engineBrakeTorque = brakingTorque * Mathf.Abs(ratio) * clutch;

            var newRpm = _gearbox.CurrentGear == 0 ? freeRpm : Mathf.Lerp(freeRpm, averageWheelsRpm * ratio, clutch);

            var producedTorque = drivingTorque * ratio * clutch;

            if (clutch < 1f)
            {
                var limit = _gearboxUsageConfig.MaxLaunchTorqueNm * clutch;
                producedTorque = Mathf.Clamp(producedTorque, -limit, limit);
            }

            _engineRpm = Mathf.SmoothDamp(_engineRpm, newRpm, ref _velocity,
                deltaTime * RPM_ADJUST_SMOOTHNESS);

            _engineRpm = Mathf.Clamp(_engineRpm, _engineConfig.IdleRPM, _engineConfig.RedlineRPM);

            return producedTorque;
        }
    }
}
