using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>Content that is ready at once; counts how often it was asked.</summary>
    internal sealed class FakeContentInitializer : IContentInitializer
    {
        public int Calls { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellation)
        {
            Calls++;

            return UniTask.CompletedTask;
        }
    }
}
