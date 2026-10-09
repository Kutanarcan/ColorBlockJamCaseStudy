using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). Content, the content
    /// update, the config, the first level (the start scene's choice, D131; meta sections load on first use), then
    /// the start's first scene: Home in the game, the tested level in a level test (D131).
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;
        private readonly ContentUpdate update;
        private readonly LoadedConfig config;
        private readonly SelectedLevel level;
        private readonly ISceneLoader scenes;
        private readonly FirstScene firstScene;

        public Bootstrapper(IContentInitializer content, ContentUpdate update, LoadedConfig config, SelectedLevel level,
            ISceneLoader scenes, FirstScene firstScene)
        {
            this.content = content;
            this.update = update;
            this.config = config;
            this.level = level;
            this.scenes = scenes;
            this.firstScene = firstScene;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await content.InitializeAsync(cancellation);
            await update.RunAsync(cancellation);
            await config.LoadAsync(cancellation);
            level.Select();
            await scenes.ReplaceContentSceneAsync(firstScene.Key, cancellation);
        }
    }
}
