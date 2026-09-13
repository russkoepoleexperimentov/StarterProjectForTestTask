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

            const float rpmToOmega = Mathf.PI * 2f / 60f;
            float omegaAtMaxPower = _engineConfig.MaxPowerRPM * rpmToOmega;
            float torqueAtMaxPower = _engineConfig.MaxPowerWatts / omegaAtMaxPower;

            if (rpm <= 0f)
                return 0f;

            if (rpm <= _engineConfig.MaxTorqueRPM)
            {
                float t = Mathf.InverseLerp(0, _engineConfig.MaxTorqueRPM, rpm);
                return Mathf.Lerp(_engineConfig.MaxTorqueNm * _engineConfig.IdleTorqueFraction, _engineConfig.MaxTorqueNm, t);
            }

            if (rpm <= _engineConfig.MaxPowerRPM)
            {
                float t = Mathf.InverseLerp(_engineConfig.MaxTorqueRPM, _engineConfig.MaxPowerRPM, rpm);
                return Mathf.Lerp(_engineConfig.MaxTorqueNm, torqueAtMaxPower, t);
            }

            if (rpm <= _engineConfig.RedlineRPM)
            {
                float t = Mathf.InverseLerp(_engineConfig.MaxPowerRPM, _engineConfig.RedlineRPM, rpm);
                return Mathf.Lerp(torqueAtMaxPower, 0f, t);
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
