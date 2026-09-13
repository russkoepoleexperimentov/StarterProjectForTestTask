using Gameplay.Car.Model;

namespace Gameplay.Car.Input
{
    public interface IInputSource {
        public DrivetrainInputModel Read();
    }
}
