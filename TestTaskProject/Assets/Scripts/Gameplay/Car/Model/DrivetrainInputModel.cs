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

        /// <summary>
        /// Сырые намерения водителя: ни фильтрации, ни коробки. Это то, что возвращает
        /// <c>CarInputHandler.ReadRaw()</c> - и у игрока, и у ИИ.
        /// </summary>
        public static DrivetrainInputModel Raw(float throttlePedal, float brakePedal, float steering,
            float clutchPedal, float handbrake)
            => new(throttlePedal, brakePedal, steering, handbrake, clutchPedal, false);

        public static DrivetrainInputModel Idle => Raw(0f, 0f, 0f, 0f, 0f);
    }
}
