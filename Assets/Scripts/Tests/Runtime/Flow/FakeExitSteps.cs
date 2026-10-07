using Game.Core;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Hands out one given step for every exit and counts the effect clears.</summary>
    internal sealed class FakeExitSteps : IExitSteps
    {
        private readonly IStep step;

        public FakeExitSteps(IStep step) => this.step = step;

        public int ClearCount { get; private set; }

        public IStep For(Block block, Direction direction) => step;

        public void ClearEffects() => ClearCount++;
    }
}
