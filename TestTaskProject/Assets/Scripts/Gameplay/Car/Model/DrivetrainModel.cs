using System;
using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    // физика двигателя и трансмиссии. Всё, что относится к интерпретации ввода
    // (коррекция газа, пассивное торможение, автокоробка, педаль сцепления),
    // живёт в Gameplay.Car.Input и приезжает сюда готовым в DrivetrainInputModel.
    public class DrivetrainModel
    {
        private readonly EngineConfig _engineConfig;
        private readonly CarSystemsConfig _carSystemsConfig;
        private readonly GearboxModel _gearbox;
        private readonly EngineTorqueCurve _torqueCurve;
        private readonly CarStateModel _stateModel;
        private readonly CarDamageModel _damageModel;
        private readonly float _idleThrottle;

        private float _engineAngularVelocity;
        private float _velocity;
        private float _time;

        private const float RPM2AngVel = Mathf.PI / 30;
        private const float AngVel2RPM = 1f / RPM2AngVel;


        public DrivetrainModel(
            EngineConfig engineConfig,
            CarSystemsConfig carSystemsConfig,
            GearboxModel gearbox,
            EngineTorqueCurve torqueCurve,
            CarStateModel stateModel,
            CarDamageModel damageModel
            )
        {
            _engineConfig = engineConfig;
            _carSystemsConfig = carSystemsConfig;
            _gearbox = gearbox;
            _torqueCurve = torqueCurve;
            _stateModel = stateModel;
            _damageModel = damageModel;

            _engineAngularVelocity = _engineConfig.IdleRPM * RPM2AngVel;
            _idleThrottle = CalculateIdleThrottle();
        }

        public DrivetrainOutputModel Tick(DrivetrainInputModel input, float averageWheelsRpm, float driveInertia, float speedKph, float deltaTime)
        {
            var acceleration = CalculateEngine(input, averageWheelsRpm, driveInertia, deltaTime);

            var brakeTorque = input.BrakePedal * _carSystemsConfig.MaxBrakeTorque;
            var handBrakeTorque = input.Handbrake * _carSystemsConfig.MaxHandBrakeTorque;
            var steerAngle = input.Steering * _carSystemsConfig.MaxSteerAngle;

            _stateModel.Set(_engineAngularVelocity * AngVel2RPM, speedKph, _gearbox.CurrentGear,
                input.ThrottlePedal, input.BrakePedal, input.Steering, input.Handbrake, input.ClutchPedal);

            return new DrivetrainOutputModel(acceleration, brakeTorque, handBrakeTorque, steerAngle);
        }

        private float CalculateEngine(DrivetrainInputModel input, float averageWheelsRpm, float driveInertia, 
            float deltaTime)
        {
            var engineRpm = _engineAngularVelocity * AngVel2RPM;
            
            
            var throttle = input.ThrottleCut ? 0f : input.ThrottlePedal;

            // повреждённый двигатель теряет отдачу, но не сопротивление - трение не масштабируем
            var grossTorque = _torqueCurve.Evaluate(engineRpm) * throttle * _damageModel.PowerMultiplier;
            var frictionTorque = _torqueCurve.EvaluateFriction(engineRpm, throttle);
            var netTorque = grossTorque - frictionTorque;
            var netTorqueImpulse = netTorque * deltaTime;

            var ratio = _gearbox.CurrentRatio;
            var clutchEngagement = Mathf.Clamp01(1f - input.ClutchPedal);
            var clutchDragImpulse = 0f;
            
            var differentialVelocity = averageWheelsRpm * RPM2AngVel;
            

            if (!Mathf.Approximately(clutchEngagement, 0) && ratio != 0)
            {
                var clutchSpeed = differentialVelocity * ratio;
                clutchDragImpulse = GetClutchDragImpulse(_engineAngularVelocity, clutchSpeed, _engineConfig.InertiaKgM, driveInertia, ratio, netTorqueImpulse, clutchEngagement, 500, deltaTime);
            }
            
            if(float.IsNaN(clutchDragImpulse)) clutchDragImpulse = 0;

            var impulseToEngine = netTorqueImpulse + clutchDragImpulse;
            var impulseToDifferential = -clutchDragImpulse * ratio;

            _engineAngularVelocity += impulseToEngine / _engineConfig.InertiaKgM;
            var differentialAccel = driveInertia > 0f ? impulseToDifferential / driveInertia : 0f;

            // prevent engine from stalling
            if (_engineAngularVelocity < _engineConfig.IdleRPM * RPM2AngVel)
            {
                _engineAngularVelocity = _engineConfig.IdleRPM * RPM2AngVel;
            }
                
            return differentialAccel;
        }

        private float CalculateIdleThrottle()
        {
            for(float throttle = 0; throttle <= 1; throttle+= 0.001f)
            {
                var grossTorque = _torqueCurve.Evaluate(_engineConfig.IdleRPM) * throttle;
                var frictionTorque = _torqueCurve.EvaluateFriction(_engineConfig.IdleRPM, throttle);

                if (grossTorque > frictionTorque)
                {
                    return throttle;
                }
            }

            Debug.LogWarning("Can't calculate idle throttle due to engine configuration");
            return 0;
        }
        
        private float GetClutchDragImpulse(
            float engineShaftSpeed, 
            float gearboxInputShaftSpeed, 
            float engineInertia, 
            float drivetrainInertia, 
            float totalRatio,
            float engineImpulse,
            float clutchEngagement, // [0..1] 0 - отжато (педаль выжата), 1 - прижато (педаль отпущена)
            float clutchTorqueCapacity,
            float deltaTime
            ){
    
            var driveInertiaAtEngine = drivetrainInertia / (totalRatio * totalRatio);
            var impulseLimit = clutchEngagement * clutchTorqueCapacity * deltaTime;
            var speedDifference = gearboxInputShaftSpeed - engineShaftSpeed;
            var impulseToMatchSpeeds = engineInertia * driveInertiaAtEngine * speedDifference;
            var engineTorqueContribution = driveInertiaAtEngine * engineImpulse;

            var idealImpulse =
                (impulseToMatchSpeeds - engineTorqueContribution)
                / (engineInertia + driveInertiaAtEngine);
    
            return Mathf.Clamp(idealImpulse, -impulseLimit, impulseLimit);
        }
    }
}
