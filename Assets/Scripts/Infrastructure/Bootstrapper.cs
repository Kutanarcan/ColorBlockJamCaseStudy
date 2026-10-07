using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's entry point: runs the start-up flow once per run (§4 Bootstrapper flow). I0 initializes
    /// content; the content update check (I3), config and save (I4, M0) and the first scene (I2, I5) follow.
    /// </summary>
    public sealed class Bootstrapper : IAsyncStartable
    {
        private readonly IContentInitializer content;

        public Bootstrapper(IContentInitializer content) => this.content = content;

        public UniTask StartAsync(CancellationToken cancellation = default) => content.InitializeAsync(cancellation);
    }
}
