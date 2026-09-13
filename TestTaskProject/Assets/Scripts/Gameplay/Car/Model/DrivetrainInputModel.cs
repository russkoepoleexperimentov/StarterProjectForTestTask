namespace Gameplay.Car.Model
{
    public class DrivetrainInputModel 
    {
        public float Throttle { get; }
        public float Brake { get; }
        public float Steering { get; }

        public DrivetrainInputModel(float throttle, float brake, float steering) {
            Throttle = throttle;
            Brake = brake;
            Steering = steering;
        }
    }
}
