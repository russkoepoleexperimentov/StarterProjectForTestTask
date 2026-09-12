using Zenject;
using UnityEngine;

public class CarUIInstaller : MonoInstaller 
{
    [SerializeField] private CarUIView _view;

    public override void InstallBindings()
    {
        Container.Bind<CarUIView>().FromInstance(_view).AsSingle();
        Container.Bind<PlayerCarRegistry>().AsSingle();
        Container.BindInterfacesAndSelfTo<CarUIController>().AsSingle();
    }
}
