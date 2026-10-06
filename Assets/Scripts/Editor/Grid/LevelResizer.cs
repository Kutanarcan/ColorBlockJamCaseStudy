using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// Grows or shrinks a level by one line on one side (D54). The line is inserted or removed just inside the
    /// frame, so that side's edge walls and doors move with the frame and everything else stays put. The frame is
    /// sealed afterwards (LevelFrame), so a grown line's two end cells become walls. Entity ids are kept.
    /// </summary>
    public static class LevelResizer
    {
        public static bool CanResize(LevelModel level, Direction side, int delta)
        {
            int size = SizeAlong(level, side) + delta;

            return (delta == 1 || delta == -1) && size >= LevelTemplates.MinSize && size <= LevelTemplates.MaxSize;
        }

        /// <summary>A new model; the given one is left as it was. Call only when <see cref="CanResize"/> holds.</summary>
        public static LevelModel Resize(LevelModel level, Direction side, int delta)
        {
            bool rows = IsRow(side);
            int line = LineOf(level, side, delta);
            LevelModel resized = rows
                ? level.WithSize(level.Width, level.Height + delta)
                : level.WithSize(level.Width + delta, level.Height);

            for (int index = 0; index < level.Width * level.Height; index++)
            {
                Cell cell = level.CellOf(index);
                int owner = level.OwnerOf(cell);
                int across = rows ? cell.Y : cell.X;

                if (owner == LevelModel.None || (delta < 0 && across == line))
                    continue;

                int moved = across > line || (delta > 0 && across == line) ? across + delta : across;
                resized.Paint(rows ? new Cell(cell.X, moved) : new Cell(moved, cell.Y), owner);
            }

            LevelFrame.Seal(resized);

            return resized;
        }

        /// <summary>Occupied cells that shrinking this side removes; frame walls at the line's two ends do not count.</summary>
        public static int CutCount(LevelModel level, Direction side)
        {
            bool rows = IsRow(side);
            int line = LineOf(level, side, -1);
            int length = rows ? level.Width : level.Height;
            int count = 0;

            for (int along = 0; along < length; along++)
            {
                int owner = level.OwnerOf(At(rows, line, along));
                bool end = along == 0 || along == length - 1;

                if (owner != LevelModel.None && !(end && level.KindOf(owner) == EntityKind.Wall))
                    count++;
            }

            return count;
        }

        /// <summary>The line inserted (grow) or removed (shrink): the one just inside the frame on that side.</summary>
        private static int LineOf(LevelModel level, Direction side, int delta)
        {
            if (side == Direction.Down || side == Direction.Left)
                return 1;

            return delta > 0 ? SizeAlong(level, side) - 1 : SizeAlong(level, side) - 2;
        }

        private static Cell At(bool rows, int line, int along) => rows ? new Cell(along, line) : new Cell(line, along);

        /// <summary>Top and bottom add or remove rows; left and right add or remove columns.</summary>
        private static bool IsRow(Direction side) => side == Direction.Up || side == Direction.Down;

        private static int SizeAlong(LevelModel level, Direction side) => IsRow(side) ? level.Height : level.Width;
    }
}
