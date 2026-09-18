using System;
using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarAudioPresenter : ITickable
    {
        private readonly EngineConfig _engineConfig;
        private readonly CarStateModel _state;
        private readonly CarAudioModel _audioModel;
        private readonly CarAudioView _audioView;

        private float[] _pitches;
        private float[] _volumes;
        private bool[] _acitves;

        public CarAudioPresenter(CarStateModel state, CarAudioModel audioModel, CarAudioView audioView,
            EngineConfig engineConfig)
        {
            _state = state;
            _audioModel = audioModel;
            _audioView = audioView;
            _engineConfig = engineConfig;
            
            var clipsNum = _engineConfig.AudioConfig?.Sounds?.Length ?? 0;
            _pitches = new float[clipsNum];
            _volumes = new float[clipsNum];
            _acitves = new bool[clipsNum];
        }

        public void Tick()
        {
            if (_engineConfig.AudioConfig)
            {
                _audioModel.UpdateClips(_state.RPM, _state.Throttle, Time.deltaTime, _pitches, _volumes, _acitves);
                _audioView.ApplyEngineClipConfiguration(_pitches, _volumes, _acitves);
            }
            
            _audioView.ApplySkidmarksSound();

            if (_audioModel.ConsumeHandbrakePull(_state.Handbrake))
                _audioView.PlayHandbrakePull();
        }
    }
}
