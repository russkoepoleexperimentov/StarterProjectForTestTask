using Gameplay.Car.Configs;
using Gameplay.Car.Input;
using Gameplay.Car.Model;
using Gameplay.Car.Presenter;
using Gameplay.Car.Services;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car
{
    public class CarInstaller : MonoInstaller
    {
        [SerializeField] private CarView _carView;
        [SerializeField] private CarAudioView _carAudioView;
        [SerializeField] private CarInputHandler _input;
        [SerializeField] private CarCollisionView _collisionView;
        [SerializeField] private CarSmokeView _smokeView;
        [SerializeField] private CarDeformationView _deformationView;

        [SerializeField] private EngineConfig _engineConfig;
        [SerializeField] private GearboxConfig _gearboxConfig;
        [SerializeField] private CarSystemsConfig _carSystemsConfig;
        [SerializeField] private CarWheelEffectsConfig _wheelEffectsConfig;
        [SerializeField] private CarDamageConfig _damageConfig;

        [SerializeField] private bool _isPlayerCar = true;

        public override void InstallBindings()
        {
            Container.Bind<EngineConfig>().FromInstance(_engineConfig).AsSingle();
            Container.Bind<GearboxConfig>().FromInstance(_gearboxConfig).AsSingle();
            Container.Bind<CarSystemsConfig>().FromInstance(_carSystemsConfig).AsSingle();
            Container.Bind<CarView>().FromInstance(_carView).AsSingle();

            // DriverConfig и стек ассистентов живут на самом обработчике: у ИИ они свои
            Container.Bind<IInputSource>().FromInstance(_input).AsSingle();

            Container.Bind<EngineTorqueCurve>().AsSingle();
            Container.Bind<GearboxModel>().AsSingle();
            Container.Bind<DrivetrainModel>().AsSingle();

            Container.Bind<CarStateModel>().AsSingle();

            Container.Bind<CarDamageConfig>().FromInstance(_damageConfig).AsSingle();
            Container.Bind<CarCollisionView>().FromInstance(_collisionView).AsSingle();
            Container.Bind<CarSmokeView>().FromInstance(_smokeView).AsSingle();
            Container.Bind<CarDeformationView>().FromInstance(_deformationView).AsSingle();
            Container.Bind<CarDamageModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<CarDamagePresenter>().AsSingle();

            Container.BindInterfacesAndSelfTo<CarAudioPresenter>().AsSingle();
            Container.Bind<CarAudioModel>().AsSingle();
            Container.Bind<CarAudioView>().FromInstance(_carAudioView).AsSingle();
            Container.Bind<CarWheelEffectsConfig>().FromInstance(_wheelEffectsConfig).AsSingle();

            Container.BindInterfacesAndSelfTo<CarPresenter>().AsSingle();

            if (_isPlayerCar)
                Container.BindInterfacesTo<PlayerCarRegistrar>().AsSingle();
        }
    }
}
