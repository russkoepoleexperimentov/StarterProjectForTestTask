using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarAudioPresenter : ITickable
    {
        private readonly CarStateModel _state;
        private readonly CarAudioModel _audioModel;
        private readonly CarAudioView _audioView;

        public CarAudioPresenter(CarStateModel state, CarAudioModel audioModel, CarAudioView audioView)
        {
            _state = state;
            _audioModel = audioModel;
            _audioView = audioView;
        }
        
        public void Tick()
        {
            var parametersModel = _audioModel.CalculateEngineAudio(_state.RPM, _state.Throttle, Time.deltaTime);
            _audioView.ApplyEngineSound(parametersModel);
            _audioView.ApplySkidmarksSound();

            if (_audioModel.ConsumeHandbrakePull(_state.Handbrake))
                _audioView.PlayHandbrakePull();
        }
    }
}
