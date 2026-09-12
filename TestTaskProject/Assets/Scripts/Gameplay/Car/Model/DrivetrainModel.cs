using Gameplay.Car.Configs;
using UnityEngine;

public class DrivetrainModel
{
    private readonly EngineConfig _engineConfig;
    private readonly GearboxConfig _gearboxConfig;
    private readonly GearboxUsageConfig _gearboxUsageConfig;
    private readonly CarStateModel _stateModel;
    private readonly CarSystemsConfig _carSystemsConfig;

    private int _currentGear;
    private float _engineRpm;
    private float _shiftTimer;
    private float _gearHoldTimer;

    private bool _passiveBraking; // механика пассивного торможения
    private float _calculatedSteeringInput;

    private float _clutchEngagement; // 0 - сцепление выжато, 1 - схвачено полностью
    private float _clutchTimeRamp;   // прогресс выпускания педали сцепления

    private float _throttleAtWindowStart;
    private float _kickdownWindowTimer;

    private const float INPUT_THRESHOLD = 0.2f;
    private const float STANDSTILL_SPEED_KPH = 1f;
    private const float RPM_ADJUST_SMOOTHNESS = 3;
    
    private float _velocity;

    public DrivetrainModel(
        EngineConfig engineConfig, 
        GearboxConfig gearboxConfig, 
        GearboxUsageConfig gearboxUsageConfig, 
        CarStateModel stateModel, 
        CarSystemsConfig carSystemsConfig
        )
    {
        _engineConfig = engineConfig;
        _gearboxConfig = gearboxConfig;
        _gearboxUsageConfig = gearboxUsageConfig;
        _stateModel = stateModel;
        _carSystemsConfig = carSystemsConfig;

        _currentGear = 0;
        _engineRpm = _engineConfig.IdleRPM;
    }

    public DrivetrainOutputModel Tick(DrivetrainInputModel input, float averageWheelsRpm, float speedKph, float deltaTime)
    {
        CalculateEngineAndTransmission(input, averageWheelsRpm, speedKph, deltaTime,  out var throttle, out var brake, out var producedTorque);
        
        // braking
        if (brake > INPUT_THRESHOLD) _passiveBraking = true;
        if (throttle > INPUT_THRESHOLD) _passiveBraking = false;
        if (_currentGear == 0) _passiveBraking = false;

        var passiveBrakingInput = _passiveBraking ? 0.2f : 0;
        var brakeInput = Mathf.Max(passiveBrakingInput, brake);
        
        var brakeTorque = brakeInput * _carSystemsConfig.MaxBrakeTorque;

        // steering
        var speedSteerFactor = Mathf.Max(1f - Mathf.Clamp01(speedKph / 60), 0.3f);
        var desiredSteerInput = speedSteerFactor * input.Steering;

        if (Mathf.Abs(_calculatedSteeringInput) < Mathf.Abs(desiredSteerInput))
        {
            // доворачиваем руль
            _calculatedSteeringInput = Mathf.MoveTowards(_calculatedSteeringInput, desiredSteerInput, deltaTime);
        }
        else
        {
            // возвращаем руль
            _calculatedSteeringInput = Mathf.MoveTowards(_calculatedSteeringInput, desiredSteerInput, deltaTime * 3);
        }
        
        var steerAngle = _calculatedSteeringInput * _carSystemsConfig.MaxSteerAngle;
        
        _stateModel.Set(_engineRpm, speedKph, _currentGear, throttle);

        return new DrivetrainOutputModel(producedTorque, brakeTorque, steerAngle);
    }

    private void CalculateEngineAndTransmission(DrivetrainInputModel input, float averageWheelsRpm, float speedKph,
        float deltaTime, out float throttle, out float brake, out float producedTorque)
    {
        // чтобы при игре с клавиатуры лучше контроллировать разгон
        var throttleCorretionFactor = 1 - Mathf.InverseLerp(_engineConfig.IdleRPM, _engineConfig.RedlineRPM, _engineRpm);
        throttleCorretionFactor *= throttleCorretionFactor;
        
        throttle = input.Throttle;
        brake = input.Brake;

        if(_currentGear < 0)
        {
            (throttle, brake) = (brake, throttle);
        }

        throttle *= throttleCorretionFactor;

        var isShifting = _shiftTimer > 0f;
        var torque = isShifting ? 0f : EvaluateTorque(_engineRpm) * throttle;
        var frictionTorque = EvaluateFrictionTorque(_engineRpm);
        var netTorque = torque - frictionTorque;
        
        
        UpdateShifting(input.Throttle, input.Brake, speedKph, torque, deltaTime);

        UpdateClutch(speedKph, isShifting, deltaTime);

        producedTorque = 0f;
        float newRpm;

        // обороты на свободном двигателе (сцепление выжато)
        var freeRpm = _engineRpm + netTorque * deltaTime / _engineConfig.InertiaKgM;

        if(_currentGear == 0 || isShifting)
        {
            newRpm = freeRpm;
        }
        else
        {
            var ratio = GetGearRatio(_currentGear);

            // чем сильнее буксует сцепление, тем меньше двигатель привязан к колёсам
            newRpm = Mathf.Lerp(freeRpm, averageWheelsRpm * ratio, _clutchEngagement);

            producedTorque = torque * ratio * _clutchEngagement;

            // пока сцепление не схвачено, момент на колёсах ограничен - иначе срыв в букс
            if (_clutchEngagement < 1f)
            {
                var limit = _gearboxUsageConfig.MaxLaunchTorqueNm;
                producedTorque = Mathf.Clamp(producedTorque, -limit, limit);
            }
        }

        _engineRpm = Mathf.SmoothDamp(_engineRpm, newRpm, ref _velocity, 
            deltaTime * RPM_ADJUST_SMOOTHNESS);

        _engineRpm = Mathf.Clamp(_engineRpm, _engineConfig.IdleRPM, _engineConfig.RedlineRPM);
    }

    private void UpdateClutch(float speedKph, bool isShifting, float deltaTime)
    {
        if (_currentGear == 0 || isShifting)
        {
            _clutchEngagement = 0f;
            _clutchTimeRamp = 0f;
            return;
        }

        var engageSpeed = _gearboxUsageConfig.ClutchEngageTimeSeconds > 0f
            ? deltaTime / _gearboxUsageConfig.ClutchEngageTimeSeconds
            : 1f;

        var target = 1f;

        // трогание с места: сцепление держим подбуксовывающим, пока машина не разогналась
        if (Mathf.Abs(_currentGear) == 1 && _gearboxUsageConfig.ClutchLockSpeedKph > 0f)
        {
            var speedFactor = Mathf.Clamp01(Mathf.Abs(speedKph) / _gearboxUsageConfig.ClutchLockSpeedKph);
            target = Mathf.Lerp(_gearboxUsageConfig.MinClutchEngagement, 1f, speedFactor);
        }

        _clutchTimeRamp = Mathf.MoveTowards(_clutchTimeRamp, 1f, engageSpeed);

        // сцепление схватывается не быстрее, чем позволяют и таймер, и набранная скорость
        _clutchEngagement = Mathf.Min(_clutchTimeRamp, target);
    }

    private float GetGearRatio(int gear)
    {
        if(gear == 0) return 0;

        var set = gear < 0 ? _gearboxConfig.BackwardGearRatios : _gearboxConfig.ForwardGearRatios;
        var index = Mathf.Abs(gear) - 1;

        return set[index] * _gearboxConfig.FinalDriveRatio;
    }

    private void UpdateShifting(float forwardInput, float backwardInput, float speedKph, float torque, float deltaTime)
    {
        if(_shiftTimer > 0f)
        {
            _shiftTimer -= deltaTime;
            return;
        }

        if(_gearHoldTimer > 0f)
            _gearHoldTimer -= deltaTime;

        var isIdleInput = forwardInput < INPUT_THRESHOLD && backwardInput < INPUT_THRESHOLD;
        var isStanding = Mathf.Abs(speedKph) < STANDSTILL_SPEED_KPH;

        if (isIdleInput && isStanding)
        {
            // включаем нейтраль
            SetGear(0);
            return;
        }

        if(_currentGear == 0 && !isIdleInput)
        {   
            // на нейтрали стартуем вперёд/назад в зависимости куда едем
            SetGear(forwardInput > backwardInput ? 1 : -1);
            return;
        }
        
        if(_shiftTimer > 0 || _gearHoldTimer > 0) return;

        var throttleInput = speedKph > 0 ? forwardInput : backwardInput;
        var absShiftDirection = _currentGear > 0 ? 1 : -1;
        var relMaxGear = _currentGear > 0 ? _gearboxConfig.ForwardGearRatios.Length : _gearboxConfig.BackwardGearRatios.Length;
        var relGear = Mathf.Abs(_currentGear);

        var torqueAtIdle = EvaluateTorque(_engineConfig.IdleRPM);
        
        var shouldShiftUp = throttleInput > 0 && _engineRpm > _gearboxUsageConfig.ShiftUpRPM && relGear < relMaxGear;
        var shouldShiftDown = torque < torqueAtIdle && _engineRpm < _gearboxUsageConfig.ShiftDownRPM && relGear > 1;

        if (shouldShiftDown)
        {
            SetGear(_currentGear - absShiftDirection);
            return;
        }

        if (shouldShiftUp)
        {
            SetGear(_currentGear + absShiftDirection);
        }
    }

    private void SetGear(int gear)
    {
        if(gear == _currentGear) return;

        _currentGear = gear;
        _shiftTimer = _gearboxUsageConfig.ShiftTimeSeconds;
        _gearHoldTimer = _gearboxUsageConfig.MinTimeInGearSeconds;
    }

    private float EvaluateTorque(float engineRpm)
    {
        float rpm = Mathf.Max(0f, engineRpm);

        const float rpmToOmega = Mathf.PI * 2f / 60f;
        float omegaAtMaxPower = _engineConfig.MaxPowerRPM * rpmToOmega;
        float torqueAtMaxPower = _engineConfig.MaxPowerWatts / omegaAtMaxPower;

        if (rpm <= 0f)
            return 0f;

        if (rpm <= _engineConfig.MaxTorqueRPM)
        {
            float t = Mathf.InverseLerp(0, _engineConfig.MaxTorqueRPM, rpm);
            return Mathf.Lerp(_engineConfig.MaxTorqueNm * _engineConfig.IdleTorqueFraction, _engineConfig.MaxTorqueNm, t);
        }

        if (rpm <= _engineConfig.MaxPowerRPM)
        {
            float t = Mathf.InverseLerp(_engineConfig.MaxTorqueRPM, _engineConfig.MaxPowerRPM, rpm);
            return Mathf.Lerp(_engineConfig.MaxTorqueNm, torqueAtMaxPower, t);
        }

        if (rpm <= _engineConfig.RedlineRPM)
        {
            float t = Mathf.InverseLerp(_engineConfig.MaxPowerRPM, _engineConfig.RedlineRPM, rpm);
            return Mathf.Lerp(torqueAtMaxPower, 0f, t);
        }

        return 0f;
    }

    private float EvaluateFrictionTorque(float engineRpm) {
        return _engineConfig.BaseFriction + engineRpm * _engineConfig.RPMFriction;
    }
}
