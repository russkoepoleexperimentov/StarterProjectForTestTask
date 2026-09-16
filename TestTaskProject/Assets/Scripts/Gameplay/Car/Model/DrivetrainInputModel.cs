namespace Gameplay.Car.Model
{
    public class DrivetrainInputModel
    {
        // педаль газа: уже развёрнута по передаче и скорректирована по оборотам
        public float ThrottlePedal { get; }
        // педаль тормоза: уже сглажена, включает пассивное торможение
        public float BrakePedal { get; }
        // отфильтрованный руль, -1..1
        public float Steering { get; }
        // рычаг ручного тормоза, 0..1
        public float Handbrake { get; }
        public float ClutchPedal { get; }
        public bool ThrottleCut { get; }

        // сырой ввод: ни фильтрации, ни коробки
        public DrivetrainInputModel(float throttlePedal, float brakePedal, float steering, float clutchPedal, float handbrake)
            : this(throttlePedal, brakePedal, steering, handbrake, clutchPedal, false)
        {
        }

        public DrivetrainInputModel(float throttlePedal, float brakePedal, float steering, float handbrake,
            float clutchPedal, bool throttleCut)
        {
            ThrottlePedal = throttlePedal;
            BrakePedal = brakePedal;
            Steering = steering;
            Handbrake = handbrake;
            ClutchPedal = clutchPedal;
            ThrottleCut = throttleCut;
        }
    }
}
