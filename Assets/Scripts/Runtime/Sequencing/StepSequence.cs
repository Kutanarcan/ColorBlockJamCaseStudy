using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>Plays its steps one after another; a cancelled token stops it before the next step.</summary>
    public sealed class StepSequence : IStep
    {
        private readonly IStep[] steps;

        public StepSequence(params IStep[] steps) => this.steps = steps;

        public async UniTask Play(CancellationToken cancellation)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                cancellation.ThrowIfCancellationRequested();
                await steps[i].Play(cancellation);
            }
        }
    }
}
