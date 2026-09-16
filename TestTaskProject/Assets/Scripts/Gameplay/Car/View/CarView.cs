using System;
using System.Collections.Generic;
using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.View
{
    public class CarView : MonoBehaviour 
    {
        [Serializable]
        public class Axle
        {
            [field: SerializeField] public CarWheelView LeftWheel { get; private set; }
            [field: SerializeField] public CarWheelView RightWheel { get; private set; }
            [field: SerializeField] public bool IsDrivable { get; private set; }
            [SerializeField] private float _brakeFactor = 1;
            [SerializeField] private float _handBrakeFactor = 0;
            [SerializeField] private float _steerFactor = 1;

            public float BrakeFactor => _brakeFactor;
            public float HandBrakeFactor => _handBrakeFactor;
            public float SteerFactor => _steerFactor;
        }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Axle[] _axles;
        [SerializeField] private Transform _centerOfMass;

        public float SpeedKph => Vector3.Dot(_rigidbody.linearVelocity, _rigidbody.transform.forward) * MPS_TO_KPH;

        public IReadOnlyCollection<Axle> Axles => _axles;

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
            float fraction = 1f / Mathf.Max(NumDriveWheels, 1);

            foreach (Axle axle in _axles)
            {
                // педаль и ручник не складываются - берём тот, что тормозит сильнее
                var brakeTorque = Mathf.Max(axle.BrakeFactor * drivetrainOutput.BrakeTorque,
                    axle.HandBrakeFactor * drivetrainOutput.HandBrakeTorque);

                if (axle.IsDrivable)
                {
                    ApplyForBothWheels(axle, w => w.MotorTorque = (w.Inertia * drivetrainOutput.AngularAcceleration) / Time.fixedDeltaTime);
                }

                ApplyForBothWheels(axle, w => w.BrakeTorque = brakeTorque);
                ApplyForBothWheels(axle, w => w.SteerAngle = axle.SteerFactor * drivetrainOutput.SteerAngle);

                ApplyForBothWheels(axle, w => w.ApplyVisual());
            }
        }

        public void FetchFeedback(out float averageDriveWheelsRpm, out float impulse, out float driveInertia)
        {
            int numDriveWheels = 0;
            averageDriveWheelsRpm = 0;
            impulse = 0;
            driveInertia = 0;
            foreach (var axle in _axles)
            {
                if (axle.IsDrivable)
                {
                    averageDriveWheelsRpm += axle.LeftWheel.RPM;
                    averageDriveWheelsRpm += axle.RightWheel.RPM;
                    impulse += axle.LeftWheel.FeedbackImpulse;
                    impulse += axle.RightWheel.FeedbackImpulse;
                    driveInertia += axle.LeftWheel.Inertia;
                    driveInertia += axle.RightWheel.Inertia;
                    numDriveWheels += 2;
                }
            }
            
            if(numDriveWheels > 0)
                averageDriveWheelsRpm /= numDriveWheels;
        }

        private void ApplyForBothWheels(Axle axle, Action<CarWheelView> func)
        {
            func(axle.LeftWheel);
            func(axle.RightWheel);
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}
