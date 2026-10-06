using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>
    /// A step the test finishes by hand: it writes "name started" to a shared log and runs until
    /// <see cref="Finish"/> or a cancel. Continuations run inline, so no frame has to pass.
    /// </summary>
    internal sealed class FakeStep : IStep
    {
        private readonly string name;
        private readonly List<string> log;
        private UniTaskCompletionSource completion;

        public FakeStep(string name, List<string> log)
        {
            this.name = name;
            this.log = log;
        }

        public bool WasCancelled { get; private set; }

        public UniTask Play(CancellationToken cancellation)
        {
            log.Add(name + " started");
            completion = new UniTaskCompletionSource();
            cancellation.Register(() => WasCancelled = completion.TrySetCanceled());

            return completion.Task;
        }

        public void Finish() => completion.TrySetResult();
    }
}
