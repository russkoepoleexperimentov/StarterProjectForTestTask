using Gameplay.Car.Configs;
using Gameplay.Car.Controller;
using Gameplay.Car.Model;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

public class CarInstaller : MonoInstaller
{
    [SerializeField] private CarView _carView;
    [SerializeField] private CarAudioView _carAudioView;
    [SerializeField] private UserInputView _input;
    
    [SerializeField] private EngineConfig _engineConfig;
    [SerializeField] private GearboxConfig _gearboxConfig;
    [SerializeField] private GearboxUsageConfig _gearboxUsageConfig;
    [SerializeField] private CarSystemsConfig _carSystemsConfig;
    [SerializeField] private CarWheelEffectsConfig _wheelEffectsConfig;
    
    [SerializeField] private CarAudioConfig _audioConfig;
    
    [SerializeField] private bool _isPlayerCar = true;

    public override void InstallBindings()
    {
        Container.Bind<EngineConfig>().FromInstance(_engineConfig).AsSingle();
        Container.Bind<GearboxConfig>().FromInstance(_gearboxConfig).AsSingle();
        Container.Bind<GearboxUsageConfig>().FromInstance(_gearboxUsageConfig).AsSingle();
        Container.Bind<CarSystemsConfig>().FromInstance(_carSystemsConfig).AsSingle();
        Container.Bind<CarView>().FromInstance(_carView).AsSingle();

        Container.Bind<IInputSource>().FromInstance(_input).AsSingle();

        Container.Bind<DrivetrainModel>().AsSingle();
        Container.Bind<CarStateModel>().AsSingle();
        
        Container.Bind<CarAudioConfig>().FromInstance(_audioConfig).AsSingle();
        Container.BindInterfacesAndSelfTo<CarAudioController>().AsSingle();
        Container.Bind<CarAudioModel>().AsSingle();
        Container.Bind<CarAudioView>().FromInstance(_carAudioView).AsSingle();
        Container.Bind<CarWheelEffectsConfig>().FromInstance(_wheelEffectsConfig).AsSingle();

        Container.BindInterfacesAndSelfTo<CarController>().AsSingle();

        if (_isPlayerCar)
            Container.BindInterfacesTo<PlayerCarRegistrar>().AsSingle();
    }
}
