using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.Input
{
    public class CarInputFilter
    {
        private readonly EngineConfig _engineConfig;
        private readonly DriverConfig _driverConfig;

        private float _brake;
        private float _steering;
        private bool _passiveBraking; // механика пассивного торможения

        public CarInputFilter(EngineConfig engineConfig, DriverConfig driverConfig)
        {
            _engineConfig = engineConfig;
            _driverConfig = driverConfig;
        }

        public float FilterThrottle(DrivetrainInputModel raw, int gear, float engineRpm)
        {
            // разворот педалей на задней передаче - не ассист, а трактовка ввода: остаётся всегда
            var throttle = SelectThrottlePedal(raw, gear);

            if (!_driverConfig.EnableThrottleAssist) return throttle;

            // чтобы при игре с клавиатуры лучше контроллировать разгон
            var correction = 1 - Mathf.InverseLerp(_engineConfig.MaxTorqueRPM, _engineConfig.RedlineRPM, engineRpm);

            return throttle * correction * correction;
        }

        public float FilterBrakePedal(DrivetrainInputModel raw, int gear, float deltaTime)
        {
            var pedal = SelectBrakePedal(raw, gear);

            if (!_driverConfig.EnableBrakeAssist)
            {
                _brake = pedal;
                return _brake;
            }

            var rate = pedal > _brake
                ? _driverConfig.BrakeRiseRate
                : _driverConfig.BrakeFallRate;

            _brake = Mathf.MoveTowards(_brake, pedal, deltaTime * rate);

            return _brake;
        }

        public float ApplyPassiveBraking(float brakePedal, float throttle, int gear)
        {
            if (!_driverConfig.EnablePassiveBraking)
            {
                _passiveBraking = false;
                return brakePedal;
            }

            if (brakePedal > _driverConfig.PedalThreshold) _passiveBraking = true;
            if (throttle > _driverConfig.PedalThreshold) _passiveBraking = false;
            if (gear == 0) _passiveBraking = false;

            var passiveBrakingInput = _passiveBraking ? _driverConfig.PassiveBrakeAmount : 0;

            return Mathf.Max(passiveBrakingInput, brakePedal);
        }

        public float FilterSteering(float rawSteering, bool handbrake, float speedKph, float deltaTime)
        {
            if (!_driverConfig.EnableSteeringAssist)
            {
                _steering = rawSteering;
                return _steering;
            }

            var speedSteerFactor = Mathf.Max(
                1f - Mathf.Clamp01(speedKph / _driverConfig.SteerSpeedFalloffKph),
                _driverConfig.MinSteerFactor);

            var desiredSteerInput = !handbrake ? speedSteerFactor * rawSteering : rawSteering;

            // доворачиваем руль медленнее, чем возвращаем
            var rate = Mathf.Abs(_steering) < Mathf.Abs(desiredSteerInput)
                ? _driverConfig.SteerAttackRate
                : _driverConfig.SteerReleaseRate;

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
