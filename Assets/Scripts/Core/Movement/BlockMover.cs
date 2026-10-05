namespace Game.Core
{
    /// <summary>
    /// Moves a block one cell. A step is valid when every target cell is empty or the block's own;
    /// walls, doors and other blocks are all just occupied. The block must still have the capability
    /// (<see cref="Capabilities"/>). A blocked step checks <see cref="ExitRule"/>.
    /// </summary>
    public sealed class BlockMover
    {
        private readonly Board board;

        public BlockMover(Board board) => this.board = board;

        public MoveResult TryMove(Block block, Direction direction)
        {
            if (block.IsExited || !Capabilities.CanMove(block, direction))
                return MoveResult.Blocked;

            Cell offset = direction.ToOffset();

            if (CanStep(block, offset))
            {
                board.MoveEntity(block, offset);

                return MoveResult.Moved;
            }

            if (Capabilities.CanExit(block) && ExitRule.CanExit(board, block, direction))
            {
                board.RemoveBlock(block);

                return MoveResult.Exited;
            }

            return MoveResult.Blocked;
        }

        private bool CanStep(Entity entity, Cell offset)
        {
            Grid grid = board.Grid;

            for (int i = 0; i < entity.CellCount; i++)
            {
                int occupant = grid.Get(entity.GetCell(i) + offset);

                if (occupant != Grid.Empty && occupant != entity.Id)
                    return false;
            }

            return true;
        }
    }
}
