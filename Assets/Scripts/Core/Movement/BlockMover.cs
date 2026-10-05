namespace Game.Core
{
    public sealed class BlockMover
    {
        private readonly Board board;

        public BlockMover(Board board) => this.board = board;

        public MoveResult TryMove(Block block, Direction direction)
        {
            if (block.IsExited || !Capabilities.CanMove(block, direction))
                return MoveResult.Blocked;

            Cell offset = direction.ToOffset();

            if (board.CanPlace(block, offset))
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
    }
}
