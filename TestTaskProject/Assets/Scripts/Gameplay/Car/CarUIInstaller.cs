using Gameplay.Car.Presenter;
using Gameplay.Car.Services;
using Gameplay.Car.View;
using UnityEngine;
using Zenject;

namespace Gameplay.Car
{
    public class CarUIInstaller : MonoInstaller 
    {
        [SerializeField] private CarUIView _view;

        public override void InstallBindings()
        {
            Container.Bind<CarUIView>().FromInstance(_view).AsSingle();
            Container.Bind<PlayerCarRegistry>().AsSingle();
            Container.BindInterfacesAndSelfTo<CarUIPresenter>().AsSingle();
        }
    }
}
