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
        private readonly DriverConfig _driverConfig;
        private readonly EngineConfig _engineConfig;
        private readonly EngineTorqueCurve _torqueCurve;

        private float _clutchEngagement; // 0 - сцепление выжато, 1 - схвачено полностью
        private float _clutchTimeRamp;   // прогресс выпускания педали сцепления

        private const float STANDSTILL_SPEED_KPH = 1f;
        private const float ANTI_STALL_RPM_MARGIN = 200f;

        public AutoGearboxAssist(
            GearboxModel gearbox,
            DriverConfig driverConfig,
            EngineConfig engineConfig,
            EngineTorqueCurve torqueCurve)
        {
            _gearbox = gearbox;
            _driverConfig = driverConfig;
            _engineConfig = engineConfig;
            _torqueCurve = torqueCurve;
        }

        public void UpdateGear(float forwardInput, float backwardInput, float speedKph,
            float torque, float engineRpm, float deltaTime)
        {
            // таймеры коробки тикают в любом случае, иначе IsShifting залипнет навсегда.
            // с выключенным ассистом передачу дёргает сам водитель через GearboxModel.Try*()
            if (_gearbox.Tick(deltaTime) || !_driverConfig.EnableAutoGearbox) return;

            var threshold = _driverConfig.PedalThreshold;
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

            var shouldShiftDown = torque < torqueAtIdle && engineRpm < _driverConfig.ShiftDownRPM;
            var shouldShiftUp = throttleInput > 0 && engineRpm > _driverConfig.ShiftUpRPM;

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

        public float UpdateClutch(float speedKph, bool isShifting, float throttle, float clutchInput, float driveWheelsRpm,
            bool handbrake, float deltaTime)
        {
            if (!_driverConfig.EnableClutchAssist)
            {
                // сцеплением работает сам водитель: отдаём его педаль как есть
                _clutchEngagement = 0f;
                _clutchTimeRamp = 0f;
                return Mathf.Clamp01(clutchInput);
            }

            // анти-заглушание: без газа, когда колёса крутят вал сцепления медленнее холостых,
            // двигатель не должен тащить машину - иначе удержание холостых толкает её с бесконечным моментом.
            // смотрим на вал, а не на двигатель: выжатый двигатель всегда на холостых и сцепление бы не вернулось
            var clutchShaftRpm = Mathf.Abs(driveWheelsRpm * _gearbox.CurrentRatio);
            var isIdleStall = throttle < _driverConfig.PedalThreshold
                              && clutchShaftRpm < _engineConfig.IdleRPM + ANTI_STALL_RPM_MARGIN;

            // ручник выжимает сцепление, чтобы не бороться с двигателем
            if (_gearbox.CurrentGear == 0 || isShifting || handbrake || isIdleStall)
            {
                _clutchEngagement = 0f;
                _clutchTimeRamp = 0f;
                return 1;
            }

            var engageSpeed = _driverConfig.ClutchEngageTimeSeconds > 0f
                ? deltaTime / _driverConfig.ClutchEngageTimeSeconds
                : 1f;

            var target = 1f;

            // трогание с места: сцепление держим подбуксовывающим, пока машина не разогналась
            if (Mathf.Abs(_gearbox.CurrentGear) == 1 && _driverConfig.ClutchLockSpeedKph > 0f)
            {
                var speedFactor = Mathf.Clamp01(Mathf.Abs(speedKph) / _driverConfig.ClutchLockSpeedKph);
                target = Mathf.Lerp(_driverConfig.MinClutchEngagement, 1f, speedFactor);
            }

            target = Mathf.Min(target, 1 - clutchInput);

            _clutchTimeRamp = Mathf.MoveTowards(_clutchTimeRamp, 1f, engageSpeed);

            // сцепление схватывается не быстрее, чем позволяют и таймер, и набранная скорость
            _clutchEngagement = Mathf.Min(_clutchTimeRamp, target);

            return 1f - _clutchEngagement; // педаль - инвертированное значение
        }
    }
}
