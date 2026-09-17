using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.Input
{
    // базовый обработчик ввода. Наследник отвечает только за то, ОТКУДА берутся
    // намерения водителя (клавиатура, ИИ), а весь стек ассистентов собирается здесь
    // по его собственному DriverConfig. Поэтому AIBrain : CarInputHandler со своим
    // ассетом конфига подключается без единой правки в остальном коде.
    public abstract class CarInputHandler : MonoBehaviour, IInputSource
    {
        [SerializeField] private DriverConfig _driverConfig;

        private CarInputProcessor _processor;

        public DriverConfig DriverConfig => _driverConfig;

        [Inject]
        public void Construct(GearboxModel gearbox, EngineConfig engineConfig,
            EngineTorqueCurve torqueCurve, CarStateModel state)
        {
            // фильтры держат состояние (сглаженные педали, схватывание сцепления),
            // поэтому они свои у каждого водителя, а не синглтоны контейнера
            var filter = new CarInputFilter(engineConfig, _driverConfig);
            var autoGearbox = new AutoGearboxAssist(gearbox, _driverConfig, engineConfig, torqueCurve);

            _processor = new CarInputProcessor(filter, autoGearbox, gearbox, torqueCurve, state);
        }

        public DrivetrainInputModel Read(float speedKph, float driveWheelsRpm, float deltaTime)
            => _processor.Process(ReadRaw(), speedKph, driveWheelsRpm, deltaTime);

        // сырые намерения водителя, без ассистентов. Собирать через DrivetrainInputModel.Raw()
        protected abstract DrivetrainInputModel ReadRaw();
    }
}
