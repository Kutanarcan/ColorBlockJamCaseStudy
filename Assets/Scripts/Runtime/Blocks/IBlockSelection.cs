using Game.Core;

namespace Game.Runtime
{
    /// <summary>Marks the block the player holds; the drag shows it on select and hides it when the drag ends.</summary>
    public interface IBlockSelection
    {
        void Show(Block block);

        void Hide();
    }
}
