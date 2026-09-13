using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.Input
{
    // ассист автоматической коробки: игрок жмёт только "вперёд"/"назад",
    // а ассист решает, когда дёрнуть коробку и насколько отпустить сцепление
    public class AutoGearboxAssist
    {
        private readonly GearboxModel _gearbox;
        private readonly GearboxUsageConfig _gearboxUsageConfig;
        private readonly CarSystemsConfig _carSystemsConfig;
        private readonly EngineConfig _engineConfig;
        private readonly EngineTorqueCurve _torqueCurve;

        private float _clutchEngagement; // 0 - сцепление выжато, 1 - схвачено полностью
        private float _clutchTimeRamp;   // прогресс выпускания педали сцепления

        private const float STANDSTILL_SPEED_KPH = 1f;

        public AutoGearboxAssist(
            GearboxModel gearbox,
            GearboxUsageConfig gearboxUsageConfig,
            CarSystemsConfig carSystemsConfig,
            EngineConfig engineConfig,
            EngineTorqueCurve torqueCurve)
        {
            _gearbox = gearbox;
            _gearboxUsageConfig = gearboxUsageConfig;
            _carSystemsConfig = carSystemsConfig;
            _engineConfig = engineConfig;
            _torqueCurve = torqueCurve;
        }

        public void UpdateGear(float forwardInput, float backwardInput, float speedKph,
            float torque, float engineRpm, float deltaTime)
        {
            if (_gearbox.Tick(deltaTime)) return;

            var threshold = _carSystemsConfig.PedalThreshold;
            var isIdleInput = forwardInput < threshold && backwardInput < threshold;
            var isStanding = Mathf.Abs(speedKph) < STANDSTILL_SPEED_KPH;

            if (isIdleInput && isStanding)
            {
                // включаем нейтраль
                _gearbox.TrySetGear(0);
                return;
            }

            if (_gearbox.CurrentGear == 0 && !isIdleInput)
            {
                // на нейтрали стартуем вперёд/назад в зависимости куда едем
                _gearbox.TrySetGear(forwardInput > backwardInput ? 1 : -1);
                return;
            }

            var throttleInput = speedKph > 0 ? forwardInput : backwardInput;
            var torqueAtIdle = _torqueCurve.Evaluate(_engineConfig.IdleRPM);

            var shouldShiftDown = torque < torqueAtIdle && engineRpm < _gearboxUsageConfig.ShiftDownRPM;
            var shouldShiftUp = throttleInput > 0 && engineRpm > _gearboxUsageConfig.ShiftUpRPM;

            if (shouldShiftDown)
            {
                _gearbox.TryShiftDown();
                return;
            }

            if (shouldShiftUp)
            {
                _gearbox.TryShiftUp();
            }
        }

        public float UpdateClutch(float speedKph, bool isShifting, float throttle, float clutchInput, float deltaTime)
        {
            if (_gearbox.CurrentGear == 0 || isShifting)
            {
                _clutchEngagement = 0f;
                _clutchTimeRamp = 0f;
                return _clutchEngagement;
            }

            var engageSpeed = _gearboxUsageConfig.ClutchEngageTimeSeconds > 0f
                ? deltaTime / _gearboxUsageConfig.ClutchEngageTimeSeconds
                : 1f;

            var target = 1f;

            // трогание с места: сцепление держим подбуксовывающим, пока машина не разогналась
            if (Mathf.Abs(_gearbox.CurrentGear) == 1 && _gearboxUsageConfig.ClutchLockSpeedKph > 0f)
            {
                var speedFactor = Mathf.Clamp01(Mathf.Abs(speedKph) / _gearboxUsageConfig.ClutchLockSpeedKph);
                target = Mathf.Lerp(_gearboxUsageConfig.MinClutchEngagement, 1f, speedFactor);
            }

            target = Mathf.Min(target, clutchInput);

            _clutchTimeRamp = Mathf.MoveTowards(_clutchTimeRamp, 1f, engageSpeed);

            // сцепление схватывается не быстрее, чем позволяют и таймер, и набранная скорость
            _clutchEngagement = Mathf.Min(_clutchTimeRamp, target);

            return _clutchEngagement;
        }
    }
}
