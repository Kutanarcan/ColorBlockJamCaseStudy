using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Fits the Arrow modifier's mark on a block (FINDINGS; AlgorithmExplanation § Arrow placement): from the
    /// block's anchor, the unbroken run of block cells along the arrow's axis; the arrow is min(run, max) long,
    /// centered on the run and laid along the axis (yaw 0 Vertical, 90 Horizontal; the mesh is double-headed).
    /// </summary>
    public static class ArrowPlacementRule
    {
        public static ArrowPlacement Of(Block block, Axis axis, int maxSize)
        {
            Cell step = axis == Axis.Vertical ? new Cell(0, 1) : new Cell(1, 0);
            Cell anchor = BlockAnchor.Of(block);
            Cell first = anchor;
            Cell last = anchor;

            while (Contains(block, first - step))
                first -= step;

            while (Contains(block, last + step))
                last += step;

            int run = Mathf.Abs(last.X - first.X) + Mathf.Abs(last.Y - first.Y) + 1;
            Vector3 center = (BoardLayout.CellCenter(first) + BoardLayout.CellCenter(last)) * 0.5f;
            float yaw = PieceYaw.Facing(axis == Axis.Vertical ? Direction.Up : Direction.Right);

            return new ArrowPlacement(new Placement(center, yaw), Mathf.Min(run, maxSize));
        }

        private static bool Contains(Block block, Cell cell)
        {
            for (int i = 0; i < block.CellCount; i++)
            {
                if (block.GetCell(i) == cell)
                    return true;
            }

            return false;
        }
    }
}
