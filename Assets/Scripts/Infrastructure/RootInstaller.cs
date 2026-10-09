using Game.Core;
using Game.LevelIO;
using Game.Meta;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's registrations, kept out of the <c>LifetimeScope</c> so a test can build them without a scene.
    /// The asset loader and the level source are scoped: every child scope gets its own, released with it (D66).
    /// The meta lives for the whole run; its save store and the level choice come from the start scene's services
    /// (D131). Every scene change goes under the loading cover, made here so both starts share it (D119, D135);
    /// what lives for the whole run is made under <c>runRoot</c>, the root scope's object.
    /// </summary>
    public sealed class RootInstaller : IInstaller
    {
        private readonly IContentInitializer content;
        private readonly IContentDelivery delivery;
        private readonly IAssetSource assets;
        private readonly ISceneLoader scenes;
        private readonly Transform runRoot;

        public RootInstaller(IContentInitializer content, IContentDelivery delivery, IAssetSource assets,
            ISceneLoader scenes, Transform runRoot)
        {
            this.content = content;
            this.delivery = delivery;
            this.assets = assets;
            this.scenes = scenes;
            this.runRoot = runRoot;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(content);
            builder.RegisterInstance(delivery);
            builder.RegisterInstance(assets);
            builder.Register<LoadedCover>(Lifetime.Singleton).As<ILoadingCover>().WithParameter(runRoot);
            // Made by hand: the loader it wraps is an ISceneLoader too, which the container would resolve to itself.
            builder.Register<ISceneLoader>(
                resolver => new CoveredSceneLoader(scenes, resolver.Resolve<ILoadingCover>()), Lifetime.Singleton);
            builder.RegisterInstance(new LevelJson(ModifierCatalog.Default()));
            builder.Register<Progression>(Lifetime.Singleton);
            builder.Register<Settings>(Lifetime.Singleton);
            builder.Register<Wallet>(Lifetime.Singleton);
            builder.Register<ContentUpdate>(Lifetime.Singleton);
            builder.Register<LoadedConfig>(Lifetime.Singleton);
            builder.Register<SelectedLevel>(Lifetime.Singleton);
            builder.Register<AssetScope>(Lifetime.Scoped).As<IAssetLoader>();
            builder.Register<AssetLevelSource>(Lifetime.Scoped).As<ILevelSource>();
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
