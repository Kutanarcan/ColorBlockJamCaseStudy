using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>Edge geometry shared by the edge rules.</summary>
    public static class EdgeCells
    {
        public static bool IsEdge(LevelModel level, Cell cell) =>
            cell.X == 0 || cell.Y == 0 || cell.X == level.Width - 1 || cell.Y == level.Height - 1;

        public static bool IsCorner(LevelModel level, Cell cell) =>
            (cell.X == 0 || cell.X == level.Width - 1) && (cell.Y == 0 || cell.Y == level.Height - 1);

        /// <summary>The direction pointing out of the grid from a non-corner edge cell.</summary>
        public static Direction Outward(LevelModel level, Cell cell)
        {
            if (cell.X == 0)
                return Direction.Left;

            if (cell.X == level.Width - 1)
                return Direction.Right;

            return cell.Y == 0 ? Direction.Down : Direction.Up;
        }
    }
}
