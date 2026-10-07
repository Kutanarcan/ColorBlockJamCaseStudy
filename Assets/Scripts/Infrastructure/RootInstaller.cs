using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's registrations, kept out of the <c>LifetimeScope</c> so a test can build them without a scene.
    /// The asset loader is scoped: every child scope gets its own <see cref="AssetScope"/>, released with it (D66).
    /// </summary>
    public sealed class RootInstaller : IInstaller
    {
        private readonly IContentInitializer content;
        private readonly IAssetSource assets;

        public RootInstaller(IContentInitializer content, IAssetSource assets)
        {
            this.content = content;
            this.assets = assets;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(content);
            builder.RegisterInstance(assets);
            builder.Register<AssetScope>(Lifetime.Scoped).As<IAssetLoader>();
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
