using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Main scene's scope (D64, M2): a child of the root scope, opened by the scene loader. It carries Home's own
    /// references and runs <see cref="MainEntry"/>; closing the scene disposes it and its asset scope. Its modal layer
    /// pauses nothing: Home has no play (D122).
    /// </summary>
    public sealed class MainLifetimeScope : LifetimeScope
    {
        [SerializeField] private ModalLayerView modalLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<IModalLayerView>(modalLayer);
            builder.Register<ModalLayer>(Lifetime.Singleton);
            builder.Register<PopupService>(Lifetime.Singleton).WithParameter<Transform>(modalLayer.transform);
            builder.RegisterEntryPoint<MainEntry>();
        }
    }
}
