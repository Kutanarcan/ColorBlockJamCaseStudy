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
        private readonly IContentDelivery delivery;
        private readonly IAssetSource assets;
        private readonly ISceneLoader scenes;

        public RootInstaller(IContentInitializer content, IContentDelivery delivery, IAssetSource assets,
            ISceneLoader scenes)
        {
            this.content = content;
            this.delivery = delivery;
            this.assets = assets;
            this.scenes = scenes;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(content);
            builder.RegisterInstance(delivery);
            builder.RegisterInstance(assets);
            builder.RegisterInstance(scenes);
            builder.Register<ContentUpdate>(Lifetime.Singleton);
            builder.Register<AssetScope>(Lifetime.Scoped).As<IAssetLoader>();
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
