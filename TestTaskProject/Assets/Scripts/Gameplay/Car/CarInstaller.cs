using Gameplay.Car.Configs;
using UnityEngine;
using Zenject;

public class CarInstaller : MonoInstaller
{
    [SerializeField] private CarView _carView;
    [SerializeField] private EngineConfig _engineConfig;
    [SerializeField] private GearboxConfig _gearboxConfig;
    [SerializeField] private GearboxUsageConfig _gearboxUsageConfig;
    [SerializeField] private UserInputView _input;
    [SerializeField] private bool _isPlayerCar = true;

    public override void InstallBindings()
    {
        Container.Bind<EngineConfig>().FromInstance(_engineConfig).AsSingle();
        Container.Bind<GearboxConfig>().FromInstance(_gearboxConfig).AsSingle();
        Container.Bind<GearboxUsageConfig>().FromInstance(_gearboxUsageConfig).AsSingle();
        Container.Bind<CarView>().FromInstance(_carView).AsSingle();

        Container.Bind<IInputSource>().FromInstance(_input).AsSingle();

        Container.Bind<DrivetrainModel>().AsSingle();
        Container.Bind<CarStateModel>().AsSingle();

        Container.BindInterfacesAndSelfTo<CarController>().AsSingle();

        if (_isPlayerCar)
            Container.BindInterfacesTo<PlayerCarRegistrar>().AsSingle();
    }
}
