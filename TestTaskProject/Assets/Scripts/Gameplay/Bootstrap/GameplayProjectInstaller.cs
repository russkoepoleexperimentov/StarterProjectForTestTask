using Gameplay.Car.Services;
using Zenject;

namespace Gameplay.Bootstrap
{
    public class GameplayProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // реестр живёт в project-контексте: HUD инстанцируется UIController'ом
            // из project-контейнера, а машины регистрируются из scene-контейнеров
            Container.Bind<PlayerCarRegistry>().AsSingle();
        }
    }
}
