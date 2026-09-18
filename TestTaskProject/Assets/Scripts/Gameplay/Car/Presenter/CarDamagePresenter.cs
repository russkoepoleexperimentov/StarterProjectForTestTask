using System;
using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarDamagePresenter : IInitializable, IDisposable, ITickable
    {
        private readonly CarDamageConfig _config;
        private readonly CarDamageModel _model;
        private readonly CarCollisionView _collisionView;
        private readonly CarAudioView _audioView;
        private readonly CarSmokeView _smokeView;
        private readonly CarDeformationView _deformationView;

        public CarDamagePresenter(
            CarDamageConfig config,
            CarDamageModel model,
            CarCollisionView collisionView,
            CarAudioView audioView,
            CarSmokeView smokeView,
            CarDeformationView deformationView)
        {
            _config = config;
            _model = model;
            _collisionView = collisionView;
            _audioView = audioView;
            _smokeView = smokeView;
            _deformationView = deformationView;
        }

        public void Initialize() => _collisionView.Impacted += HandleImpact;

        public void Dispose() => _collisionView.Impacted -= HandleImpact;

        public void Tick()
        {
            _smokeView.Apply(_model.IsSmoking, _model.SmokeIntensity01);
            _audioView.ApplySteamSound(_model.IsSmoking, _model.SmokeIntensity01);
        }

        private void HandleImpact(CarImpactModel impact)
        {
            if (!_model.TryApplyImpact(impact, Time.time, out var result)) return;

            var volume = Mathf.Lerp(_config.MinImpactVolume, _config.MaxImpactVolume, result.Force01);

            _audioView.PlayImpact(result.IsStrong, volume);
            _deformationView.Deform(result.Point, result.Normal, result.Force01);
        }
    }
}
