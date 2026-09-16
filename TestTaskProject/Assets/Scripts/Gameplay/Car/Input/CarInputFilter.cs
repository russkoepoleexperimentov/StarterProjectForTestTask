using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.Input
{
    public class CarInputFilter
    {
        private readonly EngineConfig _engineConfig;
        private readonly CarSystemsConfig _carSystemsConfig;

        private float _brake;
        private float _steering;
        private bool _passiveBraking; // механика пассивного торможения

        public CarInputFilter(EngineConfig engineConfig, CarSystemsConfig carSystemsConfig)
        {
            _engineConfig = engineConfig;
            _carSystemsConfig = carSystemsConfig;
        }

        public float FilterThrottle(DrivetrainInputModel raw, int gear, float engineRpm)
        {
            var throttle = SelectThrottlePedal(raw, gear);

            // чтобы при игре с клавиатуры лучше контроллировать разгон
            var correction = 1 - Mathf.InverseLerp(_engineConfig.MaxTorqueRPM, _engineConfig.RedlineRPM, engineRpm);

            return throttle * correction * correction;
        }

        public float FilterBrakePedal(DrivetrainInputModel raw, int gear, float deltaTime)
        {
            var pedal = SelectBrakePedal(raw, gear);

            var rate = pedal > _brake
                ? _carSystemsConfig.BrakeRiseRate
                : _carSystemsConfig.BrakeFallRate;

            _brake = Mathf.MoveTowards(_brake, pedal, deltaTime * rate);

            return _brake;
        }

        public float ApplyPassiveBraking(float brakePedal, float throttle, int gear)
        {
            if (brakePedal > _carSystemsConfig.PedalThreshold) _passiveBraking = true;
            if (throttle > _carSystemsConfig.PedalThreshold) _passiveBraking = false;
            if (gear == 0) _passiveBraking = false;

            var passiveBrakingInput = _passiveBraking ? _carSystemsConfig.PassiveBrakeAmount : 0;

            return Mathf.Max(passiveBrakingInput, brakePedal);
        }

        public float FilterSteering(float rawSteering, bool handbrake, float speedKph, float deltaTime)
        {
            var speedSteerFactor = Mathf.Max(
                1f - Mathf.Clamp01(speedKph / _carSystemsConfig.SteerSpeedFalloffKph),
                _carSystemsConfig.MinSteerFactor);

            var desiredSteerInput = !handbrake ? speedSteerFactor * rawSteering : rawSteering;

            // доворачиваем руль медленнее, чем возвращаем
            var rate = Mathf.Abs(_steering) < Mathf.Abs(desiredSteerInput)
                ? _carSystemsConfig.SteerAttackRate
                : _carSystemsConfig.SteerReleaseRate;

            _steering = Mathf.MoveTowards(_steering, desiredSteerInput, deltaTime * rate);

            return _steering;
        }

        // на задней передаче педали меняются местами: газ всегда едет в сторону передачи
        private static float SelectThrottlePedal(DrivetrainInputModel raw, int gear)
            => gear < 0 ? raw.BrakePedal : raw.ThrottlePedal;

        private static float SelectBrakePedal(DrivetrainInputModel raw, int gear)
            => gear < 0 ? raw.ThrottlePedal : raw.BrakePedal;
    }
}
