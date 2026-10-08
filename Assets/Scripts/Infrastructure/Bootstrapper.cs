using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). Content, the content
    /// update, the config, then the first scene; the save (M0) and the editor's level (I5) join it.
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;
        private readonly ContentUpdate update;
        private readonly IAssetLoader assets;
        private readonly LoadedConfig config;
        private readonly ISceneLoader scenes;

        public Bootstrapper(IContentInitializer content, ContentUpdate update, IAssetLoader assets, LoadedConfig config,
            ISceneLoader scenes)
        {
            this.content = content;
            this.update = update;
            this.assets = assets;
            this.config = config;
            this.scenes = scenes;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await content.InitializeAsync(cancellation);
            await update.RunAsync(cancellation);
            config.Set(await assets.LoadAsync<GameConfig>(GameConfig.Key, cancellation));
            await scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, cancellation);
        }
    }
}
