using Gameplay.Car.Model;

namespace Gameplay.Car.Input
{
    public class CarInputProcessor
    {
        private readonly CarInputFilter _filter;
        private readonly AutoGearboxAssist _autoGearbox;
        private readonly GearboxModel _gearbox;
        private readonly EngineTorqueCurve _torqueCurve;
        private readonly CarStateModel _state;

        public CarInputProcessor(
            CarInputFilter filter,
            AutoGearboxAssist autoGearbox,
            GearboxModel gearbox,
            EngineTorqueCurve torqueCurve,
            CarStateModel state)
        {
            _filter = filter;
            _autoGearbox = autoGearbox;
            _gearbox = gearbox;
            _torqueCurve = torqueCurve;
            _state = state;
        }

        public DrivetrainInputModel Process(DrivetrainInputModel raw, float speedKph, float deltaTime)
        {
            // снимаем ДО тика коробки: это же значение уходит и в отсечку газа, и в сцепление
            var isShifting = _gearbox.IsShifting;
            var engineRpm = _state.RPM;

            // педали разворачиваем по ещё не переключённой передаче
            var gearBeforeShift = _gearbox.CurrentGear;
            var throttle = _filter.FilterThrottle(raw, gearBeforeShift, engineRpm);
            var brakePedal = _filter.FilterBrakePedal(raw, gearBeforeShift, deltaTime);

            // момент для условия "пора вниз"; на переключении газ отрезан
            var torque = isShifting ? 0f : _torqueCurve.Evaluate(engineRpm) * throttle;

            // коробке отдаём сырые намерения игрока
            _autoGearbox.UpdateGear(raw.Throttle, raw.Brake, speedKph, torque, engineRpm, deltaTime);

            // а вот пассивное торможение считаем уже на новой передаче
            var brake = _filter.ApplyPassiveBraking(brakePedal, throttle, _gearbox.CurrentGear);

            var handbrake = raw.Handbrake;
            var clutch = _autoGearbox.UpdateClutch(speedKph, isShifting, throttle, raw.ClutchEngagement, deltaTime);
            var steering = _filter.FilterSteering(raw.Steering, handbrake > 0, speedKph, deltaTime);

            return new DrivetrainInputModel(throttle, brake, steering, handbrake, clutch, isShifting);
        }
    }
}
