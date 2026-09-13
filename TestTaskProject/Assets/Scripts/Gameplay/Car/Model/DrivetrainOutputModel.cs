namespace Gameplay.Car.Model
{
    public class DrivetrainOutputModel
    {
        public float MotorTorque { get; }
        // торможение от педали
        public float BrakeTorque { get; }
        // торможение от ручника
        public float HandBrakeTorque { get; }
        // торможение двигателем, прикладывается только к ведущим колёсам
        public float EngineBrakeTorque { get; }
        public float SteerAngle { get; }

        public DrivetrainOutputModel(float motorTorque, float brakeTorque, float handBrakeTorque,
            float engineBrakeTorque, float steerAngle) {
            MotorTorque = motorTorque;
            BrakeTorque = brakeTorque;
            HandBrakeTorque = handBrakeTorque;
            EngineBrakeTorque = engineBrakeTorque;
            SteerAngle = steerAngle;
        }
    }
}
