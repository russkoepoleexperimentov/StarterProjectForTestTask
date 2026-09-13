namespace Gameplay.Car.Model
{
    public class DrivetrainOutputModel
    {
        public float MotorTorque { get; }
        public float BrakeTorque { get; }
        public float SteerAngle { get; }

        public DrivetrainOutputModel(float motorTorque, float brakeTorque, float steerAngle) {
            MotorTorque = motorTorque;
            BrakeTorque = brakeTorque;
            SteerAngle = steerAngle;
        }
    }
}
