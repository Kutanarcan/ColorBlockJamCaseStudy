using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>Starts all its steps together and finishes when the last one does.</summary>
    public sealed class StepGroup : IStep
    {
        private readonly IStep[] steps;

        public StepGroup(params IStep[] steps) => this.steps = steps;

        public UniTask Play(CancellationToken cancellation)
        {
            var playing = new UniTask[steps.Length];

            for (int i = 0; i < steps.Length; i++)
                playing[i] = steps[i].Play(cancellation);

            return UniTask.WhenAll(playing);
        }
    }
}
