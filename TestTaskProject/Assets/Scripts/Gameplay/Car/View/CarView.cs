using UnityEngine;

public class CarView : MonoBehaviour 
{
    [SerializeField] private WheelCollider[] _driveWheels;

    public float AverageWheelsRpm {
        get {
            float rpm = 0;
            foreach (var wheel in _driveWheels)
            {
                rpm += wheel.rpm;
            }
            return rpm / Mathf.Max(_driveWheels.Length, 1);
        }
    }

    public void ApplyDrive(DrivetrainOutputModel drivetrainOutput) 
    {
        foreach (var wheel in _driveWheels)
        {
            wheel.motorTorque = drivetrainOutput.MotorTorque;
        }
    }
}