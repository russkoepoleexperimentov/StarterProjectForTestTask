using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    public class EngineTorqueCurve
    {
        private readonly EngineConfig _engineConfig;

        public EngineTorqueCurve(EngineConfig engineConfig)
        {
            _engineConfig = engineConfig;
        }

        public float Evaluate(float engineRpm)
        {
            float rpm = Mathf.Max(0f, engineRpm);

            if (rpm <= 0f)
                return 0f;

            const float rpmToOmega = Mathf.PI * 2f / 60f;

            float omegaAtMaxPower =
                _engineConfig.MaxPowerRPM * rpmToOmega;

            float torqueAtMaxPower =
                _engineConfig.MaxPowerWatts / omegaAtMaxPower;

            float maxTorque = _engineConfig.MaxTorqueNm;
            float maxTorqueRpm = _engineConfig.MaxTorqueRPM;
            float maxPowerRpm = _engineConfig.MaxPowerRPM;
            float redlineRpm = _engineConfig.RedlineRPM;

            if (rpm <= maxTorqueRpm)
            {
                float t = Mathf.InverseLerp(0f, maxTorqueRpm, rpm);

                float idleTorque =
                    maxTorque * _engineConfig.IdleTorqueFraction;

                float curve = t * t;

                return Mathf.Lerp(idleTorque, maxTorque, curve);
            }

            if (rpm <= maxPowerRpm)
            {
                float t = Mathf.InverseLerp(
                    maxTorqueRpm,
                    maxPowerRpm,
                    rpm);

                float curve = 1f - t * t;

                return Mathf.Lerp(
                    torqueAtMaxPower,
                    maxTorque,
                    curve);
            }

            if (rpm <= redlineRpm)
            {
                float t = Mathf.InverseLerp(
                    maxPowerRpm,
                    redlineRpm,
                    rpm);

                float curve = (1f - t) * (1f - t);

                return torqueAtMaxPower * curve;
            }

            return 0f;
        }
        public float EvaluateFriction(float engineRpm, float throttle)
        {
            var friction = _engineConfig.BaseFriction + engineRpm * _engineConfig.RPMFriction;

            return friction * (1f - Mathf.Clamp01(throttle));
        }

        public float EvaluateEngineBrakeFade(float engineRpm)
        {
            if (_engineConfig.EngineBrakeFadeRPM <= 0f)
                return 1f;

            return Mathf.Clamp01((engineRpm - _engineConfig.IdleRPM) / _engineConfig.EngineBrakeFadeRPM);
        }
    }
}
