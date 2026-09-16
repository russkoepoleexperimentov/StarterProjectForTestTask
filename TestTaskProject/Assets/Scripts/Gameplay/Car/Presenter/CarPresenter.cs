using Gameplay.Car.Input;
using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarPresenter : IFixedTickable {
        private readonly CarView _view;
        private readonly DrivetrainModel _drivetrainModel;
        private readonly CarInputProcessor _inputProcessor;
        private readonly IInputSource _input;

        public CarPresenter(CarView view, DrivetrainModel drivetrainModel, CarInputProcessor inputProcessor, IInputSource input) {
            _drivetrainModel = drivetrainModel;
            _inputProcessor = inputProcessor;
            _view = view;
            _input = input;
        }

        public void FixedTick()
        {
            _view.FetchFeedback(out var averageDriveWheelsRpm, out var feedbackImpulse, out var driveInertia);
            var raw = _input.Read();
            var input = _inputProcessor.Process(raw, _view.SpeedKph, Time.fixedDeltaTime);
            var output = _drivetrainModel.Tick(input, averageDriveWheelsRpm, feedbackImpulse, driveInertia, _view.SpeedKph, Time.fixedDeltaTime);

            _view.ApplyDrive(output);
        }
    }
}
