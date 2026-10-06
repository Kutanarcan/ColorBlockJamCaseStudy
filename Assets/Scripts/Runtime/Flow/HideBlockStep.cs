using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>Placeholder exit: the block disappears at once. P6 replaces it with the exit step (snap, slide, cut).</summary>
    public sealed class HideBlockStep : IStep
    {
        private readonly BlockView view;

        public HideBlockStep(BlockView view) => this.view = view;

        public UniTask Play(CancellationToken cancellation)
        {
            view.Hide();

            return UniTask.CompletedTask;
        }
    }
}
