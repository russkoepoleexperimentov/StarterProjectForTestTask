using System;
using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    // состояние повреждений двигателя и кузова. Решает, какой удар считается ударом,
    // сколько он стоит здоровья и во что это превращается для мощности/дыма/деформации.
    public class CarDamageModel
    {
        private readonly CarDamageConfig _config;

        private float _engineHealth;
        private float _lastImpactTime = float.NegativeInfinity;

        public float EngineHealth => _engineHealth;
        public float EngineHealth01 => _config.MaxEngineHealth > 0f
            ? Mathf.Clamp01(_engineHealth / _config.MaxEngineHealth)
            : 0f;

        public event Action<CarDamageModel> Changed;

        public CarDamageModel(CarDamageConfig config)
        {
            _config = config;
            _engineHealth = _config.MaxEngineHealth;
        }

        public float PowerMultiplier
        {
            get
            {
                var curve = _config.PowerLossCurve;
                // пустая кривая в ассете не должна глушить целый двигатель
                var t = curve != null && curve.length > 0
                    ? Mathf.Clamp01(curve.Evaluate(EngineHealth01))
                    : EngineHealth01;

                return Mathf.Lerp(_config.MinPowerMultiplier, 1f, t);
            }
        }

        public bool IsSmoking => EngineHealth01 < _config.SmokeHealthThreshold;

        // 0 на самом пороге дымления, 1 при полностью убитом двигателе
        public float SmokeIntensity01 => _config.SmokeHealthThreshold > 0f
            ? Mathf.Clamp01(1f - EngineHealth01 / _config.SmokeHealthThreshold)
            : 1f;

        public bool TryApplyImpact(in CarImpactModel impact, float time, out CarImpactResult result)
        {
            result = default;

            if (impact.Impulse < _config.MinImpactImpulse) return false;
            if (time - _lastImpactTime < _config.ImpactCooldown) return false;

            _lastImpactTime = time;

            var damage = Mathf.Clamp(impact.Impulse * _config.DamagePerImpulse, 0f, _config.MaxDamagePerImpact);
            _engineHealth = Mathf.Max(0f, _engineHealth - damage);

            var force01 = Mathf.Clamp01(Mathf.InverseLerp(_config.MinImpactImpulse,
                _config.StrongImpactImpulse, impact.Impulse));

            result = new CarImpactResult(impact.Impulse >= _config.StrongImpactImpulse, force01,
                impact.Point, impact.Normal);

            Changed?.Invoke(this);

            return true;
        }

        public void Restore(float engineHealth)
        {
            _engineHealth = Mathf.Clamp(engineHealth, 0f, _config.MaxEngineHealth);
            Changed?.Invoke(this);
        }
    }
}
