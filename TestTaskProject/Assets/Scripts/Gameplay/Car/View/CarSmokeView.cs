using Gameplay.Car.Configs;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.View
{
    // якорь для дыма из-под капота: сам префаб собирается в редакторе и лежит в конфиге
    public class CarSmokeView : MonoBehaviour
    {
        private ParticleSystem _smoke;
        private ParticleSystem.EmissionModule _emission;
        private CarDamageConfig _config;

        private bool _isPlaying;

        [Inject]
        public void Init(CarDamageConfig config)
        {
            _config = config;

            if (_config.SmokePrefab == null) return;

            _smoke = Instantiate(_config.SmokePrefab, transform.position, transform.rotation, transform);

            var main = _smoke.main;
            main.playOnAwake = false;

            _emission = _smoke.emission;

            _smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Apply(bool active, float intensity01)
        {
            if (_smoke == null) return;

            if (active)
            {
                _emission.rateOverTimeMultiplier =
                    Mathf.Lerp(_config.SmokeMinEmission, _config.SmokeMaxEmission, Mathf.Clamp01(intensity01));

                if (!_isPlaying)
                {
                    _smoke.Play(true);
                    _isPlaying = true;
                }
            }
            else if (_isPlaying)
            {
                // уже выпущенные частицы догорают сами
                _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _isPlaying = false;
            }
        }
    }
}
