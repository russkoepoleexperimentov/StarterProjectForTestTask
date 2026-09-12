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

    private float _throttleAtWindowStart;
    private float _kickdownWindowTimer;

    private const float INPUT_THRESHOLD = 0.2f;
    private const float STANDSTILL_SPEED_KPH = 1f;
    private const float RPM_ADJUST_SMOOTHNESS = 0.3f;
    
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
        // чтобы при игре с клавиатуры лучше контроллировать разгон
        var throttleCorretionFactor = 1 - Mathf.InverseLerp(_engineConfig.IdleRPM, _engineConfig.RedlineRPM, _engineRpm);
        throttleCorretionFactor *= throttleCorretionFactor;
        
        var throttle = input.Throttle * throttleCorretionFactor;
        var brake = input.Brake;

        if(_currentGear < 0)
        {
            (throttle, brake) = (brake, throttle);
        }

        var isShifting = _shiftTimer > 0f;
        var torque = isShifting ? 0f : EvaluateTorque(_engineRpm) * throttle;
        var frictionTorque = EvaluateFrictionTorque(_engineRpm);
        var netTorque = torque - frictionTorque;
        
        
        UpdateShifting(input.Throttle, input.Brake, speedKph, torque, deltaTime);

        var producedTorque = 0f;
        float newRpm;

        if(_currentGear == 0 || isShifting)
        {
            newRpm = _engineRpm + netTorque * deltaTime / _engineConfig.InertiaKgM;
        }
        else
        {
            var ratio = GetGearRatio(_currentGear);
            
            newRpm = averageWheelsRpm * ratio;
            producedTorque = torque * ratio;
        }

        _engineRpm = Mathf.SmoothDamp(_engineRpm, newRpm, ref _velocity, 
            deltaTime * RPM_ADJUST_SMOOTHNESS);
        
        // clutch start simulation =))
        float minClutchRPM = _engineConfig.IdleRPM;
        if (Mathf.Abs(_currentGear) == 1) // 1 or R1
            minClutchRPM += throttle * 1500;
        if (_engineRpm < minClutchRPM)
            _engineRpm = minClutchRPM;

        _engineRpm = Mathf.Clamp(_engineRpm, _engineConfig.IdleRPM, _engineConfig.RedlineRPM);

        _stateModel.Set(_engineRpm, speedKph, _currentGear);
        
        // systems
        var brakeTorque = brake * _carSystemsConfig.MaxBrakeTorque;
        var steerAngle = input.Steering * _carSystemsConfig.MaxSteerAngle;

        return new DrivetrainOutputModel(producedTorque, brakeTorque, steerAngle);
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
