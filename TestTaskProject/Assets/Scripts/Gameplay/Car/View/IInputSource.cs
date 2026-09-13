using Gameplay.Car.Model;

namespace Gameplay.Car.View
{
    public interface IInputSource {
        public DrivetrainInputModel Read();
    }
}
