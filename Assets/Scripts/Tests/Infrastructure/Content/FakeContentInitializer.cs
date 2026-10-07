using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>
    /// Content that is ready at once, or, when held, only once the test calls <see cref="Finish"/>. Counts how often
    /// it was asked.
    /// </summary>
    internal sealed class FakeContentInitializer : IContentInitializer
    {
        private readonly UniTaskCompletionSource ready = new UniTaskCompletionSource();

        public FakeContentInitializer(bool held = false)
        {
            if (!held)
                ready.TrySetResult();
        }

        public int Calls { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellation)
        {
            Calls++;

            return ready.Task;
        }

        public void Finish() => ready.TrySetResult();
    }
}
