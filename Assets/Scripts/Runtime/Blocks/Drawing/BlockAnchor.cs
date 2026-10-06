using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// The block's most central cell: the one nearest its bounding-box center, first wins a tie. On a concave
    /// shape the box center can be empty; snapping to a block cell keeps marks (arrow, ice count) on the block.
    /// </summary>
    public static class BlockAnchor
    {
        public static Cell Of(Block block)
        {
            GetBounds(block, out Cell min, out Cell max);

            // Doubled units keep the box center on whole numbers: cell center = 2x + 1, box center = min + max + 1.
            int centerX = min.X + max.X + 1;
            int centerY = min.Y + max.Y + 1;
            Cell anchor = block.GetCell(0);
            int best = int.MaxValue;

            for (int i = 0; i < block.CellCount; i++)
            {
                Cell cell = block.GetCell(i);
                int dx = 2 * cell.X + 1 - centerX;
                int dy = 2 * cell.Y + 1 - centerY;
                int distance = dx * dx + dy * dy;

                if (distance < best)
                {
                    best = distance;
                    anchor = cell;
                }
            }

            return anchor;
        }

        private static void GetBounds(Block block, out Cell min, out Cell max)
        {
            min = max = block.GetCell(0);

            for (int i = 1; i < block.CellCount; i++)
            {
                Cell cell = block.GetCell(i);
                min = new Cell(System.Math.Min(min.X, cell.X), System.Math.Min(min.Y, cell.Y));
                max = new Cell(System.Math.Max(max.X, cell.X), System.Math.Max(max.Y, cell.Y));
            }
        }
    }
}
