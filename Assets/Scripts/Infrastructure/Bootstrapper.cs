using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). Content, the content
    /// update, then the first scene; config and save (I4, M0) and the editor's level (I5) join it.
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;
        private readonly ContentUpdate update;
        private readonly ISceneLoader scenes;

        public Bootstrapper(IContentInitializer content, ContentUpdate update, ISceneLoader scenes)
        {
            this.content = content;
            this.update = update;
            this.scenes = scenes;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await content.InitializeAsync(cancellation);
            await update.RunAsync(cancellation);
            await scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, cancellation);
        }
    }
}
