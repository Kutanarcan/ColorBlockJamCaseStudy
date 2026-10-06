using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>The exit's view of one block: its <see cref="BlockView"/>, plus the burst of its color.</summary>
    public sealed class BlockExitView : IExitView
    {
        private readonly BlockView view;
        private readonly ExitBurst burst;
        private readonly ExitPath path;
        private readonly int colorId;
        private readonly MaterialPropertyBlock clipBuffer;

        public BlockExitView(BlockView view, ExitBurst burst, ExitPath path, int colorId,
            MaterialPropertyBlock clipBuffer)
        {
            this.view = view;
            this.burst = burst;
            this.path = path;
            this.colorId = colorId;
            this.clipBuffer = clipBuffer;
        }

        public void SetClipPlane(Vector4 plane) => view.SetClipPlane(plane, clipBuffer);

        public UniTask MoveTo(Vector3 boardPosition, float duration, Ease ease, CancellationToken cancellation) =>
            view.MoveTo(boardPosition, duration, ease, cancellation);

        public void BurstRow(int row) => burst.Emit(path, row, colorId);

        public void Hide() => view.Hide();
    }
}
