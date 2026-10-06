using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Dresses every solid cell (wall or door) with 1×1 wall-kit pieces, one decision per quadrant (D93).
    /// A quadrant looks at its two side neighbors (h, v) and the diagonal one (d) in its own direction:
    /// both sides open → outer Corner, one side open → Wall facing it, only d open → inner Corner,
    /// nothing open → empty. The frame, inner walls and notches all follow from this; nothing is stretched.
    /// Quadrants a door covers on its approach side are left to <see cref="DoorDrawRule"/>.
    /// </summary>
    public static class WallDrawRule
    {
        private const float QuadrantOffset = BoardLayout.CellSize * 0.25f;

        private static readonly int[] SignsX = { -1, 1, -1, 1 };
        private static readonly int[] SignsZ = { -1, -1, 1, 1 };

        public static void Build(BoardLayout layout, List<Placement> walls, List<Placement> corners)
        {
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    var cell = new Cell(x, y);

                    if (layout.IsSolid(cell))
                        DressCell(layout, cell, walls, corners);
                }
            }
        }

        private static void DressCell(BoardLayout layout, Cell cell, List<Placement> walls, List<Placement> corners)
        {
            DoorData door = layout.DoorAt(cell);

            for (int i = 0; i < SignsX.Length; i++)
            {
                int signX = SignsX[i];
                int signZ = SignsZ[i];

                if (door == null || !DoorDrawRule.Covers(door, signX, signZ))
                    DressQuadrant(layout, cell, signX, signZ, walls, corners);
            }
        }

        private static void DressQuadrant(BoardLayout layout, Cell cell, int signX, int signZ,
            List<Placement> walls, List<Placement> corners)
        {
            bool sideXOpen = !layout.IsSolid(cell + new Cell(signX, 0));
            bool sideZOpen = !layout.IsSolid(cell + new Cell(0, signZ));
            Vector3 position = QuadrantCenter(cell, signX, signZ);

            if (sideXOpen && sideZOpen)
                corners.Add(new Placement(position, PieceYaw.CornerRoundedToward(signX, signZ)));
            else if (sideXOpen)
                walls.Add(new Placement(position, PieceYaw.Facing(signX > 0 ? Direction.Right : Direction.Left)));
            else if (sideZOpen)
                walls.Add(new Placement(position, PieceYaw.Facing(signZ > 0 ? Direction.Up : Direction.Down)));
            else if (!layout.IsSolid(cell + new Cell(signX, signZ)))
                corners.Add(new Placement(position, PieceYaw.CornerRoundedToward(-signX, -signZ)));
        }

        private static Vector3 QuadrantCenter(Cell cell, int signX, int signZ) =>
            BoardLayout.CellCenter(cell) + new Vector3(signX * QuadrantOffset, 0f, signZ * QuadrantOffset);
    }
}
