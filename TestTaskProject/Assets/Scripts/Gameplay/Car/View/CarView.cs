using UnityEngine;

public class CarView : MonoBehaviour 
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private WheelCollider[] _driveWheels;
    [SerializeField] private Transform _centerOfMass;

    public float SpeedKph => Vector3.Dot(_rigidbody.linearVelocity, _rigidbody.transform.forward) * MPS_TO_KPH;
    public int NumDriveWheels => _driveWheels.Length;

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

    private const float MPS_TO_KPH = 3.6f;

    private void Start()
    {
        if (_centerOfMass != null)
        {
            _rigidbody.centerOfMass = transform.InverseTransformPoint(_centerOfMass.position);
        }
    }

    public void ApplyDrive(DrivetrainOutputModel drivetrainOutput) 
    {
        foreach (var wheel in _driveWheels)
        {
            wheel.motorTorque = drivetrainOutput.MotorTorque * _driveWheels.Length;
        }
    }

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
