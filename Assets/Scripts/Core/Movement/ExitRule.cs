namespace Game.Core
{
    /// <summary>
    /// Exit check for a blocked step: for every line the block covers along the direction, the first
    /// occupied cell ahead of the line's front cell must be a door of the block's color facing that direction.
    /// </summary>
    internal static class ExitRule
    {
        public static bool CanExit(Board board, Block block, Direction direction)
        {
            Cell offset = direction.ToOffset();

            for (int i = 0; i < block.CellCount; i++)
            {
                if (!LineAllowsExit(board, block, block.GetCell(i), offset, direction))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Scans forward from one block cell, skipping empty cells (recesses). Reaching the block's own
        /// cell means <paramref name="from"/> is not the front of its line; the front cell's scan decides.
        /// Terminates because the grid's outer ring is walls and doors.
        /// </summary>
        private static bool LineAllowsExit(Board board, Block block, Cell from, Cell offset, Direction direction)
        {
            Grid grid = board.Grid;
            Cell cell = from + offset;
            int occupant = grid.Get(cell);

            while (occupant == Grid.Empty)
            {
                cell += offset;
                occupant = grid.Get(cell);
            }

            if (occupant == block.Id)
                return true;

            return board.GetEntity(occupant) is Door door
                && door.Direction == direction
                && door.ColorId == block.ColorId;
        }
    }
}
