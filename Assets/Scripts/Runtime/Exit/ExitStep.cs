using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// A block's exit (FINDINGS): cut at the door, snap onto its cell (drops the lift and the drag offset), crunch,
    /// then slide through the door at a fixed speed and disappear. The slide is split at each row's burst point, so
    /// every row bursts exactly when its center reaches the cut, whatever the speed.
    /// </summary>
    public sealed class ExitStep : IStep
    {
        private readonly IExitView view;
        private readonly ExitPath path;
        private readonly ExitSettings settings;
        private readonly ISfxPlayer sfx;

        public ExitStep(IExitView view, ExitPath path, ExitSettings settings, ISfxPlayer sfx)
        {
            this.view = view;
            this.path = path;
            this.settings = settings;
            this.sfx = sfx;
        }

        public async UniTask Play(CancellationToken cancellation)
        {
            view.SetClipPlane(path.ClipPlane);
            await view.MoveTo(path.Rest, settings.SnapDuration, Ease.OutQuad, cancellation);
            sfx.Play(SoundEffect.Crunch);

            float slid = 0f;

            for (int row = 0; row < path.RowCount; row++)
            {
                float distance = path.DistanceToRow(row);
                await SlideTo(distance - slid, distance, cancellation);
                view.BurstRow(row);
                slid = distance;
            }

            await SlideTo(path.TotalDistance - slid, path.TotalDistance, cancellation);
            view.Hide();
        }

        private UniTask SlideTo(float length, float distance, CancellationToken cancellation)
        {
            Vector3 target = path.Rest + path.Normal * distance;

            return view.MoveTo(target, length / settings.SlideSpeed, Ease.Linear, cancellation);
        }
    }
}
