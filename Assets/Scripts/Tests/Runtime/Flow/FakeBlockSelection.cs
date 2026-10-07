using Game.Core;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Remembers the block shown, if any.</summary>
    internal sealed class FakeBlockSelection : IBlockSelection
    {
        public Block Shown { get; private set; }

        public void Show(Block block) => Shown = block;

        public void Hide() => Shown = null;
    }
}
