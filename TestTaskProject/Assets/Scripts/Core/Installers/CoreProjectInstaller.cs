using UnityEngine;
using Zenject;

public class CoreProjectInstaller : MonoInstaller
{
    [SerializeField] private UIController _uiController;

    public override void InstallBindings()
    {
        Container.Bind<EventManager>().AsSingle();
        // с UIController общаются только через шину событий, поэтому он не биндится
        Container.QueueForInject(_uiController);
        Container.Bind<LoadingState>().AsSingle();
        Container.Bind<LoadingService>().AsSingle().NonLazy();
    }
}
