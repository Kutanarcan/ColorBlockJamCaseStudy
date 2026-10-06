using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>Waits until another sequencer has nothing running: the win popup waits for the last exit (D89).</summary>
    public sealed class WaitForIdleStep : IStep
    {
        private readonly Sequencer sequencer;

        public WaitForIdleStep(Sequencer sequencer) => this.sequencer = sequencer;

        public UniTask Play(CancellationToken cancellation) =>
            sequencer.WhenIdle().AttachExternalCancellation(cancellation);
    }
}
