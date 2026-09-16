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
        private readonly GearboxUsageConfig _gearboxUsageConfig;
        private readonly CarSystemsConfig _carSystemsConfig;
        private readonly GearboxModel _gearbox;
        private readonly EngineTorqueCurve _torqueCurve;
        private readonly CarStateModel _stateModel;

        private float _engineAngularVelocity;
        private float _differentialVelocity;
        private float _velocity;

        private const float RPM2AngVel = Mathf.PI / 30;
        private const float AngVel2RPM = 1f / RPM2AngVel;

        public DrivetrainModel(
            EngineConfig engineConfig,
            GearboxUsageConfig gearboxUsageConfig,
            CarSystemsConfig carSystemsConfig,
            GearboxModel gearbox,
            EngineTorqueCurve torqueCurve,
            CarStateModel stateModel
            )
        {
            _engineConfig = engineConfig;
            _gearboxUsageConfig = gearboxUsageConfig;
            _carSystemsConfig = carSystemsConfig;
            _gearbox = gearbox;
            _torqueCurve = torqueCurve;
            _stateModel = stateModel;

            _engineAngularVelocity = _engineConfig.IdleRPM * RPM2AngVel;
        }

        public DrivetrainOutputModel Tick(DrivetrainInputModel input, float averageWheelsRpm, float feedbackImpulse, float driveInertia, float speedKph, float deltaTime)
        {
            var acceleration = CalculateEngine(input, averageWheelsRpm, feedbackImpulse, driveInertia, deltaTime);

            var brakeTorque = input.BrakePedal * _carSystemsConfig.MaxBrakeTorque;
            var handBrakeTorque = input.Handbrake * _carSystemsConfig.MaxHandBrakeTorque;
            var steerAngle = input.Steering * _carSystemsConfig.MaxSteerAngle;

            _stateModel.Set(_engineAngularVelocity * AngVel2RPM, speedKph, _gearbox.CurrentGear,
                input.ThrottlePedal, input.BrakePedal, input.Steering, input.Handbrake, input.ClutchPedal);

            return new DrivetrainOutputModel(acceleration, brakeTorque, handBrakeTorque, steerAngle);
        }

        private float CalculateEngine(DrivetrainInputModel input, float averageWheelsRpm, float feedbackImpulse, float driveInertia, 
            float deltaTime)
        {
            var throttle = input.ThrottleCut ? 0f : input.ThrottlePedal;

            var engineRpm = _engineAngularVelocity * AngVel2RPM;
            var grossTorque = _torqueCurve.Evaluate(engineRpm) * throttle;
            var frictionTorque = _torqueCurve.EvaluateFriction(engineRpm, throttle);
            var netTorque = grossTorque - frictionTorque;
            var netTorqueImpulse = netTorque * deltaTime;

            var ratio = _gearbox.CurrentRatio;
            var clutchEngagement = Mathf.Clamp01(1f - input.ClutchPedal);
            var clutchDragImpulse = 0f;
            

            if (!Mathf.Approximately(clutchEngagement, 0) && ratio != 0 && !Mathf.Approximately(feedbackImpulse, 0))
            {
                var clutchSpeed = _differentialVelocity * ratio;
                clutchDragImpulse = GetClutchDragImpulse(_engineAngularVelocity, clutchSpeed, _engineConfig.InertiaKgM, driveInertia, ratio, netTorqueImpulse, feedbackImpulse, clutchEngagement, 500, deltaTime);
            }
            
            if(float.IsNaN(clutchDragImpulse)) clutchDragImpulse = 0;

            var impulseToEngine = netTorqueImpulse + clutchDragImpulse;
            var impulseToDifferential = feedbackImpulse - clutchDragImpulse * ratio;
            
            _engineAngularVelocity += impulseToEngine / _engineConfig.InertiaKgM;
            _differentialVelocity += impulseToDifferential / driveInertia;

            if (float.IsNaN(_differentialVelocity)) _differentialVelocity = 0f;
            
            var differentialRealVelocity = averageWheelsRpm * RPM2AngVel;
            var correctionAcceleration = _differentialVelocity - differentialRealVelocity;

            _engineAngularVelocity = Mathf.Clamp(_engineAngularVelocity, _engineConfig.IdleRPM * RPM2AngVel,
                _engineConfig.RedlineRPM * RPM2AngVel);

            if (float.IsNaN(_engineAngularVelocity)) _engineAngularVelocity = _engineConfig.IdleRPM * RPM2AngVel;
            if (float.IsNaN(correctionAcceleration)) correctionAcceleration = 0;
            
            Debug.Log(
                $" Feedback impulse: {feedbackImpulse}\n" +
            $" drive inertia: {driveInertia}\n" +
            $"Clutch drag: {clutchDragImpulse}\n" +
            $"Accumulated accel: {correctionAcceleration}"
                       );
                
            return correctionAcceleration;
        }
        
        
        private float GetClutchDragImpulse(
            float engineShaftSpeed, 
            float gearboxInputShaftSpeed, 
            float engineInertia, 
            float drivetrainInertia, 
            float totalRatio,
            float engineImpulse,
            float wheelImpulse,
            float clutchEngagement, // [0..1] 0 - отжато (педаль выжата), 1 - прижато (педаль отпущена)
            float clutchTorqueCapacity,
            float deltaTime
            ){
    
            var driveInertiaAtEngine = drivetrainInertia / (totalRatio * totalRatio);
            var impulseLimit = clutchEngagement * clutchTorqueCapacity * deltaTime;
            var speedDifference = gearboxInputShaftSpeed - engineShaftSpeed;
            var impulseToMatchSpeeds = engineInertia * driveInertiaAtEngine * speedDifference;
            
            // учёт фактического импульса от колёс
            var wheelImpulseAtEngine = engineInertia * (wheelImpulse / totalRatio);
            var engineTorqueContribution = driveInertiaAtEngine * engineImpulse;
            
            var idealImpulse =
                (impulseToMatchSpeeds - wheelImpulseAtEngine + engineTorqueContribution)
                / (engineInertia + driveInertiaAtEngine);
    
            return Mathf.Clamp(idealImpulse, -impulseLimit, impulseLimit);
        }
    }
}
