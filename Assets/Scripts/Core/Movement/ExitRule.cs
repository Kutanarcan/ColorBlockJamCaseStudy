namespace Game.Core
{
    internal static class ExitRule
    {
        public static bool CanExit(Board board, Block block, Direction direction)
        {
            Cell offset = direction.ToOffset();
            int color = Colors.Of(block);

            for (int i = 0; i < block.CellCount; i++)
            {
                if (!LineAllowsExit(board, block, color, block.GetCell(i), offset, direction))
                    return false;
            }

            return true;
        }

        private static bool LineAllowsExit(Board board, Block block, int color, Cell from, Cell offset,
            Direction direction)
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
                && Colors.Of(door) == color;
        }
    }
}
