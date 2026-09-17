using Gameplay.Car.Model;

namespace Gameplay.Car.Input
{
    // источник ввода для машины: игрок, ИИ, реплей. Отдаёт уже обработанный ввод -
    // ассистенты принадлежат водителю, поэтому применяет их он сам, а не презентер.
    public interface IInputSource
    {
        DrivetrainInputModel Read(float speedKph, float driveWheelsRpm, float deltaTime);
    }
}
