using System;
using Gameplay.Car.View;
using UnityEngine;

public class CarView : MonoBehaviour 
{
    [Serializable]
    public class Axle
    {
        [field: SerializeField] public CarWheelView LeftWheel { get; private set; }
        [field: SerializeField] public CarWheelView RightWheel { get; private set; }
        [field: SerializeField] public bool IsDrivable { get; private set; }
    }
    
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Axle[] _axles;
    [SerializeField] private Transform _centerOfMass;

    public float SpeedKph => Vector3.Dot(_rigidbody.linearVelocity, _rigidbody.transform.forward) * MPS_TO_KPH;

    public int NumDriveWheels
    {
        get
        {
            int result = 0;
            foreach (var axle in _axles)
            {
                if (axle.IsDrivable)
                {
                    result += 2;
                }
            }
            return result;
        }
    }

    public float AverageWheelsRpm {
        get {
            float rpm = 0;
            foreach (var axle in _axles)
            {
                if (axle.IsDrivable)
                {
                    rpm += axle.LeftWheel.Collider.rpm;
                    rpm += axle.RightWheel.Collider.rpm;
                }
            }
            return rpm / Mathf.Max(NumDriveWheels, 1);
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
        float fraction = 1f / NumDriveWheels;
        
        foreach (Axle axle in _axles)
        {
            if (axle.IsDrivable)
            {
                axle.LeftWheel.Collider.motorTorque = drivetrainOutput.MotorTorque * fraction;
                axle.RightWheel.Collider.motorTorque = drivetrainOutput.MotorTorque * fraction;
            }
            
            axle.LeftWheel.ApplyVisual();
            axle.RightWheel.ApplyVisual();
        }
    }

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
