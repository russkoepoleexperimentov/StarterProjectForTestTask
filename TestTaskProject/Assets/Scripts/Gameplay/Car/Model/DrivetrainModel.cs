using UnityEngine;

public class DrivetrainModel 
{
    private readonly EngineConfig _engineConfig;
    private readonly GearboxConfig _gearboxConfig;

    private readonly float _bestShiftUpRpm;
    private readonly float _bestShiftDownRpm;

    private int _currentGear;
    private float _engineRpm;

    public DrivetrainModel(EngineConfig engineConfig, GearboxConfig gearboxConfig)
    {
        _engineConfig = engineConfig;
        _gearboxConfig = gearboxConfig;
        _currentGear = 0;

        _bestShiftDownRpm = GetBestShiftDownRPM();
        _bestShiftUpRpm = GetBestShiftUpRPM();
    }

    public DrivetrainOutputModel Tick(DrivetrainInputModel input, float averageWheelsRpm, float deltaTime)
    {
        UpdateShifting(_engineRpm, input.Throttle, input.Brake);

        var throttle = input.Throttle;
        var brake = input.Brake;

        if(_currentGear < 0)
        {
            (throttle, brake) = (brake, throttle);
        }

        var torque = EvaluateTorque(_engineRpm) * throttle;
        var frictionTorque = EvaluateFrictionTorque(_engineRpm);
        var netTorque = torque - frictionTorque;

        var producedTorque = 0f;

        if(_currentGear == 0) 
        {
            _engineRpm += netTorque * deltaTime / _engineConfig.InertiaKgM;
        }
        else 
        {
            var ratio = GetCurrentGearRatio();

            var wheelsRpm = averageWheelsRpm * ratio;
            _engineRpm = wheelsRpm;

            producedTorque = torque;
        }

        _engineRpm = Mathf.Clamp(_engineRpm, _engineConfig.IdleRPM, _engineConfig.RedlineRPM);

        return new DrivetrainOutputModel(producedTorque, 0, 0);
    }

    private float GetCurrentGearRatio() 
    {
        if(_currentGear >= _gearboxConfig.ForwardGearRatios.Length || 
            -_currentGear > _gearboxConfig.BackwardGearRatios.Length) 
            return 0;

        if(_currentGear == 0) return 0; // neutral gear case

        if(_currentGear > 0) 
            return _gearboxConfig.ForwardGearRatios[_currentGear] * _gearboxConfig.FinalDriveRatio;

        return _gearboxConfig.BackwardGearRatios[-_currentGear] * _gearboxConfig.FinalDriveRatio;
    }

    private void UpdateShifting(float currentRpm, float rawThrottle, float rawBrake) 
    {
        if(_currentGear != 0) 
        {
            if(currentRpm > _bestShiftUpRpm){
                ShiftRelative(1);
            }
            else if(currentRpm > _bestShiftDownRpm) {
                ShiftRelative(-1);
            }
        }
        else 
        {
            const float inputThreshold = 0.2f;
            int desiredDirection = rawThrottle < inputThreshold && rawBrake < inputThreshold ? 0 :
                rawThrottle > rawBrake ? 1 : -1;

            if(desiredDirection > 0)
                ShiftAbsolute(1);
            else if (desiredDirection < 0)
                ShiftAbsolute(-1);
        }
    }

    private void ShiftRelative(int direction) {
        var maxGear = _currentGear > 0 ? _gearboxConfig.ForwardGearRatios.Length - 1 : _gearboxConfig.BackwardGearRatios.Length - 1;
        _currentGear = (int)Mathf.Sign(_currentGear) * Mathf.Clamp(Mathf.Abs(_currentGear) + (int)Mathf.Sign(direction), 0, maxGear);
    }

    private void ShiftAbsolute(int direction) {
        var minGear = -(_gearboxConfig.BackwardGearRatios.Length - 1);
        var maxGear = _gearboxConfig.ForwardGearRatios.Length - 1;
        _currentGear = (int)Mathf.Clamp(_currentGear + (int)Mathf.Sign(direction), minGear, maxGear);
    }

    private float EvaluateTorque(float engineRpm) 
    {
        float rpm = Mathf.Max(0f, engineRpm);

        const float rpmToOmega = Mathf.PI * 2f / 60f;
        float omegaAtMaxPower = _engineConfig.MaxPowerRPM * rpmToOmega;
        float torqueAtMaxPower = _engineConfig.MaxPowerWatts / omegaAtMaxPower;

        if (rpm <= 0f)
            return 0f;

        if (rpm < _engineConfig.IdleRPM)
        {
            float t = rpm / _engineConfig.IdleRPM;
            return Mathf.Lerp(0f, _engineConfig.MaxTorqueNm * _engineConfig.IdleTorqueFraction, t);
        }

        if (rpm <= _engineConfig.MaxTorqueRPM)
        {
            float t = Mathf.InverseLerp(_engineConfig.IdleRPM, _engineConfig.MaxTorqueRPM, rpm);
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

    private float GetBestShiftUpRPM() => 2500; // todo
    private float GetBestShiftDownRPM() => 800; // todo
}
