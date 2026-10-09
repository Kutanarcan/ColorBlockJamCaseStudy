using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>
    /// Refreshes the modifier looks once an exiting block lands in its door. The session tells the view about an exit
    /// before the exit's consequences run (D85), so Ice's count is read here, after the snap, when the logic is done.
    /// One instance serves every exit.
    /// </summary>
    public sealed class RefreshLooksStep : IStep
    {
        private readonly IBlocksView blocks;
        private readonly TimeSpan landing;

        public RefreshLooksStep(IBlocksView blocks, float landingSeconds)
        {
            this.blocks = blocks;
            landing = TimeSpan.FromSeconds(landingSeconds);
        }

        public async UniTask Play(CancellationToken cancellation)
        {
            await UniTask.Delay(landing, cancellationToken: cancellation);
            blocks.RefreshLooks();
        }
    }
}
