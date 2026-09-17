using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    // физическая коробка: хранит включённую передачу, считает передаточное число
    // и таймеры. Кто именно переключает - автомат, кнопки игрока - её не касается,
    // снаружи дёргают Try*()-ручки, а она решает, принять запрос или отклонить.
    public class GearboxModel
    {
        private readonly GearboxConfig _gearboxConfig;

        private float _shiftTimer;
        private float _gearHoldTimer;

        // <0 - задние, 0 - нейтраль, >0 - передние
        public int CurrentGear { get; private set; }

        public bool IsShifting => _shiftTimer > 0f;
        public float CurrentRatio => GetGearRatio(CurrentGear);

        public int MaxForwardGear => _gearboxConfig.ForwardGearRatios.Length;
        public int MaxReverseGear => _gearboxConfig.BackwardGearRatios.Length;

        public GearboxModel(GearboxConfig gearboxConfig)
        {
            _gearboxConfig = gearboxConfig;

            CurrentGear = 0;
        }

        // true - тик ушёл на переключение, решений в этот тик не принимаем
        public bool Tick(float deltaTime)
        {
            if (_shiftTimer > 0f)
            {
                _shiftTimer -= deltaTime;
                return true;
            }

            if (_gearHoldTimer > 0f)
                _gearHoldTimer -= deltaTime;

            return false;
        }

        // нейтраль и трогание с места: таймер удержания передачи здесь не действует
        public bool TrySetGear(int gear)
        {
            if (gear == CurrentGear) return false;
            if (IsShifting) return false;
            if (gear > MaxForwardGear || gear < -MaxReverseGear) return false;

            CurrentGear = gear;
            _shiftTimer = _gearboxConfig.ShiftTimeSeconds;
            _gearHoldTimer = _gearboxConfig.MinTimeInGearSeconds;

            return true;
        }

        // "вверх" - дальше от нейтрали, в ту же сторону, куда уже включена передача
        public bool TryShiftUp() => TryShiftBy(1);

        public bool TryShiftDown() => TryShiftBy(-1);

        private bool TryShiftBy(int step)
        {
            if (CurrentGear == 0) return false;
            if (_gearHoldTimer > 0f) return false;

            var relGear = Mathf.Abs(CurrentGear) + step;

            if (relGear < 1) return false;

            var direction = CurrentGear > 0 ? 1 : -1;

            return TrySetGear(relGear * direction);
        }

        private float GetGearRatio(int gear)
        {
            if (gear == 0) return 0;

            var set = gear < 0 ? _gearboxConfig.BackwardGearRatios : _gearboxConfig.ForwardGearRatios;
            var index = Mathf.Abs(gear) - 1;

            return set[index] * _gearboxConfig.FinalDriveRatio;
        }
    }
}
