using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). Content, the content
    /// update, the config, the first level (the Level Editor's request wins over progression; meta sections load on
    /// first use), then the first scene.
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;
        private readonly ContentUpdate update;
        private readonly LoadedConfig config;
        private readonly SelectedLevel level;
        private readonly ISceneLoader scenes;

        public Bootstrapper(IContentInitializer content, ContentUpdate update, LoadedConfig config, SelectedLevel level,
            ISceneLoader scenes)
        {
            this.content = content;
            this.update = update;
            this.config = config;
            this.level = level;
            this.scenes = scenes;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await content.InitializeAsync(cancellation);
            await update.RunAsync(cancellation);
            await config.LoadAsync(cancellation);
            level.SelectFirst();
            await scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, cancellation);
        }
    }
}
