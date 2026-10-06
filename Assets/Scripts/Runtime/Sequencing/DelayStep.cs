using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>Waits a number of seconds.</summary>
    public sealed class DelayStep : IStep
    {
        private readonly float seconds;

        public DelayStep(float seconds) => this.seconds = seconds;

        public UniTask Play(CancellationToken cancellation) =>
            UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellation);
    }
}
