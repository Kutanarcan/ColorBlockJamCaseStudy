using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). Content first, then
    /// the first scene; the content update check (I3), config and save (I4, M0) and the editor's level (I5) join it.
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;
        private readonly ISceneLoader scenes;

        public Bootstrapper(IContentInitializer content, ISceneLoader scenes)
        {
            this.content = content;
            this.scenes = scenes;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await content.InitializeAsync(cancellation);
            await scenes.ReplaceContentSceneAsync(SceneKeys.Gameplay, cancellation);
        }
    }
}
