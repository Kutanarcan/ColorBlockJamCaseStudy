using Game.Meta;
using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The services the game runs on (D131): the player's save, the level progression points to, the configured
    /// starting coins, and Home as the first scene. A later cross-cutting service (analytics, live events) registers its
    /// real implementation here.
    /// </summary>
    public sealed class LiveServicesInstaller : IInstaller
    {
        private readonly ISaveStore save;

        public LiveServicesInstaller(ISaveStore save) => this.save = save;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(save);
            builder.Register<ProgressionLevelChoice>(Lifetime.Singleton).As<ILevelChoice>();
            builder.Register<ConfigStartingCoins>(Lifetime.Singleton).As<IStartingCoins>();
            builder.RegisterInstance(new FirstScene(SceneKeys.Main));
        }
    }
}
