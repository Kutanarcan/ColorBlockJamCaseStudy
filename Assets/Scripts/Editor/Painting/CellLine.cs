using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// The cells between two drag points, each a side neighbour of the one before, so a fast drag still paints a
    /// connected line: a block stays one shape, a door stays unbroken.
    /// </summary>
    public static class CellLine
    {
        /// <summary>
        /// Fills <paramref name="result"/> with the cells after <paramref name="from"/> up to and including
        /// <paramref name="to"/>. Steps along x while (0.5 + steps x) / dx &lt; (0.5 + steps y) / dy, in integers.
        /// </summary>
        public static void Fill(Cell from, Cell to, List<Cell> result)
        {
            result.Clear();

            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            var stepX = new Cell(Math.Sign(to.X - from.X), 0);
            var stepY = new Cell(0, Math.Sign(to.Y - from.Y));
            int error = dx - dy;
            Cell cell = from;

            for (int remaining = dx + dy; remaining > 0; remaining--)
            {
                if (error > 0)
                {
                    cell += stepX;
                    error -= 2 * dy;
                }
                else
                {
                    cell += stepY;
                    error += 2 * dx;
                }

                result.Add(cell);
            }
        }
    }
}
