using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// The frame: the grid's edge cells (D58). An edge cell is a wall by default and holds only walls and doors;
    /// a block never goes there, and a frame cell that would become empty turns back into a wall.
    /// </summary>
    public static class LevelFrame
    {
        private static readonly Cell[] Neighbours = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };

        public static bool Allows(LevelModel level, Cell cell, EntityKind kind) =>
            kind != EntityKind.Block || !EdgeCells.IsEdge(level, cell);

        /// <summary>Turns every empty edge cell into a wall.</summary>
        public static void Seal(LevelModel level)
        {
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    var cell = new Cell(x, y);

                    if (EdgeCells.IsEdge(level, cell) && level.OwnerOf(cell) == LevelModel.None)
                        level.Paint(cell, WallFor(level, cell));
                }
            }
        }

        /// <summary>The wall an edge cell joins: a frame wall beside it, else any frame wall, else a new wall.</summary>
        public static int WallFor(LevelModel level, Cell cell)
        {
            foreach (Cell offset in Neighbours)
            {
                Cell next = cell + offset;

                if (level.Contains(next) && EdgeCells.IsEdge(level, next) && IsWall(level, next))
                    return level.OwnerOf(next);
            }

            for (int index = 0; index < level.Width * level.Height; index++)
            {
                Cell edge = level.CellOf(index);

                if (EdgeCells.IsEdge(level, edge) && IsWall(level, edge))
                    return level.OwnerOf(edge);
            }

            return level.AddWall();
        }

        private static bool IsWall(LevelModel level, Cell cell)
        {
            int owner = level.OwnerOf(cell);

            return owner != LevelModel.None && level.KindOf(owner) == EntityKind.Wall;
        }
    }
}
