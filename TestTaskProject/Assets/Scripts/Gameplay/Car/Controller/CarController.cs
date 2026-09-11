using UnityEngine;
using Zenject;

public class CarController : IFixedTickable {
    private readonly CarView _view;
    private readonly DrivetrainModel _drivetrainModel;
    private readonly IInputSource _input;

    public CarController(CarView view, DrivetrainModel drivetrainModel, IInputSource input) {
        _drivetrainModel = drivetrainModel;
        _view = view;
        _input = input;
    }

    public void FixedTick()
    {
          var input  = _input.Read();
          var output = _drivetrainModel.Tick(input, _view.AverageWheelsRpm, Time.fixedDeltaTime);

          _view.ApplyDrive(output);
    }
}