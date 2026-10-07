using System.Collections.Generic;
using Game.Core;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Records builds and clears; has no block views, so nothing may drag or exit through it.</summary>
    internal sealed class FakeBlocksView : IBlocksView
    {
        public List<string> Calls { get; } = new List<string>();
        public Board LastBuilt { get; private set; }

        public void Build(Board board)
        {
            LastBuilt = board;
            Calls.Add("build");
        }

        public void Clear() => Calls.Add("clear");

        public BlockView ViewOf(Block block) => null;
    }
}
