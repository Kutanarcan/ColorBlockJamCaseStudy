namespace Game.Core
{
    public sealed class BlockMover
    {
        private readonly Board board;

        public BlockMover(Board board) => this.board = board;

        /// <summary>
        /// What <see cref="TryMove"/> would do, without doing it: the board does not change. The one place the move
        /// rule lives; <see cref="TryMove"/> only applies its answer.
        /// </summary>
        public MoveResult Preview(Block block, Direction direction)
        {
            if (block.IsExited || !Capabilities.CanMove(block, direction))
                return MoveResult.Blocked;

            if (board.CanPlace(block, direction.ToOffset()))
                return MoveResult.Moved;

            if (Capabilities.CanExit(block) && ExitRule.CanExit(board, block, direction))
                return MoveResult.Exited;

            return MoveResult.Blocked;
        }

        public MoveResult TryMove(Block block, Direction direction)
        {
            MoveResult result = Preview(block, direction);

            if (result == MoveResult.Moved)
                board.MoveEntity(block, direction.ToOffset());
            else if (result == MoveResult.Exited)
                board.RemoveBlock(block);

            return result;
        }
    }
}
