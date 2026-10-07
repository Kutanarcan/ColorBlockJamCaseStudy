using Game.Core;

namespace Game.Runtime
{
    /// <summary>The blocks on screen as the drag, the exits and the restart reach them.</summary>
    public interface IBlocksView
    {
        void Build(Board board);

        /// <summary>Returns every part to its pool and removes every block; nothing of the last build stays.</summary>
        void Clear();

        BlockView ViewOf(Block block);
    }
}
