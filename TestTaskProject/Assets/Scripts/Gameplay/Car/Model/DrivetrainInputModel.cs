namespace Gameplay.Car.Model
{
    public class DrivetrainInputModel
    {
        // педаль газа: уже развёрнута по передаче и скорректирована по оборотам
        public float Throttle { get; }
        // педаль тормоза: уже сглажена, включает пассивное торможение
        public float Brake { get; }
        // отфильтрованный руль, -1..1
        public float Steering { get; }
        // рычаг ручного тормоза, 0..1
        public float Handbrake { get; }
        // 0 - сцепление выжато, 1 - схвачено полностью
        public float ClutchEngagement { get; }
        // коробка отрезала газ на время переключения
        public bool ThrottleCut { get; }

        // сырой ввод: ни фильтрации, ни коробки
        public DrivetrainInputModel(float throttle, float brake, float steering, float clutchEngagement, float handbrake)
            : this(throttle, brake, steering, handbrake, clutchEngagement, false)
        {
        }

        public DrivetrainInputModel(float throttle, float brake, float steering, float handbrake,
            float clutchEngagement, bool throttleCut)
        {
            Throttle = throttle;
            Brake = brake;
            Steering = steering;
            Handbrake = handbrake;
            ClutchEngagement = clutchEngagement;
            ThrottleCut = throttleCut;
        }
    }
}
