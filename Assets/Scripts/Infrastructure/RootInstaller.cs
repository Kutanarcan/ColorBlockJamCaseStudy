using Game.Core;
using Game.LevelIO;
using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's registrations, kept out of the <c>LifetimeScope</c> so a test can build them without a scene.
    /// The asset loader and the level source are scoped: every child scope gets its own, released with it (D66).
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
            builder.RegisterInstance(new LevelJson(ModifierCatalog.Default()));
            builder.Register<ContentUpdate>(Lifetime.Singleton);
            builder.Register<LoadedConfig>(Lifetime.Singleton);
            builder.Register<EditorPlayRequestStore>(Lifetime.Singleton).As<IPlayRequestStore>();
            builder.Register<PlayRequest>(Lifetime.Singleton);
            builder.Register<SelectedLevel>(Lifetime.Singleton);
            builder.Register<AssetScope>(Lifetime.Scoped).As<IAssetLoader>();
            builder.Register<AssetLevelSource>(Lifetime.Scoped).As<ILevelSource>();
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
