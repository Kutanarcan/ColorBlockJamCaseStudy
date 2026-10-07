using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Builds the exit step of a block that just left the board, so the director needs one dependency for exits.
    /// Reads the logic (cells, color) at the moment of the exit; the step then plays without it.
    /// </summary>
    public sealed class ExitSteps : IExitSteps
    {
        private readonly IBlocksView blocks;
        private readonly ExitBurst burst;
        private readonly ISfxPlayer sfx;
        private readonly ExitSettings settings;

        public ExitSteps(IBlocksView blocks, ExitBurst burst, ISfxPlayer sfx, ExitSettings settings)
        {
            this.blocks = blocks;
            this.burst = burst;
            this.sfx = sfx;
            this.settings = settings;
        }

        public IStep For(Block block, Direction direction)
        {
            var path = new ExitPath(block, direction, settings.ClipOffset);
            var view = new BlockExitView(blocks.ViewOf(block), burst, path, Colors.Of(block));

            return new ExitStep(view, path, settings, sfx);
        }

        public void ClearEffects() => burst.Clear();
    }
}
