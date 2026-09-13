using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Presenter
{
    public class CarPresenter : IFixedTickable {
        private readonly CarView _view;
        private readonly DrivetrainModel _drivetrainModel;
        private readonly IInputSource _input;

        public CarPresenter(CarView view, DrivetrainModel drivetrainModel, IInputSource input) {
            _drivetrainModel = drivetrainModel;
            _view = view;
            _input = input;
        }

        public void FixedTick()
        {
            var input  = _input.Read();
            var output = _drivetrainModel.Tick(input, _view.AverageWheelsRpm, _view.SpeedKph, Time.fixedDeltaTime);

            _view.ApplyDrive(output);
        }
    }
}
