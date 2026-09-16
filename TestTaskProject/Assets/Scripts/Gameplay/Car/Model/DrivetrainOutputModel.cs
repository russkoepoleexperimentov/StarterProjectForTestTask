namespace Gameplay.Car.Model
{
    public class DrivetrainOutputModel
    {
        public float AngularAcceleration { get; }
        public float BrakeTorque { get; }
        public float HandBrakeTorque { get; }
        public float SteerAngle { get; }

        public DrivetrainOutputModel(float angularAcceleration, float brakeTorque, float handBrakeTorque, float steerAngle) {
            AngularAcceleration = angularAcceleration;
            BrakeTorque = brakeTorque;
            HandBrakeTorque = handBrakeTorque;
            SteerAngle = steerAngle;
        }
    }
}
