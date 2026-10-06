using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Dresses a block's cells with BlockParts meshes (FINDINGS shape; AlgorithmExplanation § BlockDrawRule).
    /// Each quadrant looks at its side neighbors X, Z and the diagonal D:
    /// X and Z but no D → one InnerCorner on the vertex, placed by this cell only (the other two skip it);
    /// neither X nor Z → OuterCorner; X and Z and D → Center; one of X / Z → Edge facing the empty side.
    /// At yaw 0 every part covers its pivot's (+X, +Z) quadrant, so a pivot offset moves it onto its quadrant.
    /// </summary>
    public static class BlockDrawRule
    {
        private const float Quadrant = BoardLayout.CellSize * 0.25f;
        private const float HalfCell = BoardLayout.CellSize * 0.5f;

        private static readonly int[] SignsX = { 1, 1, -1, -1 };
        private static readonly int[] SignsZ = { 1, -1, -1, 1 };
        private static readonly Vector3 Footprint = new Vector3(Quadrant, 0f, Quadrant);

        public static void Build(HashSet<Cell> cells, List<BlockPart> parts)
        {
            foreach (Cell cell in cells)
            {
                for (int i = 0; i < SignsX.Length; i++)
                    DressQuadrant(cells, cell, SignsX[i], SignsZ[i], parts);
            }
        }

        private static void DressQuadrant(HashSet<Cell> cells, Cell cell, int signX, int signZ, List<BlockPart> parts)
        {
            bool hasX = cells.Contains(cell + new Cell(signX, 0));
            bool hasZ = cells.Contains(cell + new Cell(0, signZ));
            bool hasDiagonal = cells.Contains(cell + new Cell(signX, signZ));
            Vector3 center = BoardLayout.CellCenter(cell);

            // One side and the diagonal: the InnerCorner placed by the cell diagonal to the empty one covers it.
            if (hasDiagonal && hasX != hasZ)
                return;

            if (hasX && hasZ)
                parts.Add(hasDiagonal
                    ? OnQuadrant(BlockPartKind.Center, center, signX, signZ, 0f)
                    : OnVertex(center, signX, signZ));
            else if (hasX || hasZ)
                parts.Add(OnQuadrant(BlockPartKind.Edge, center, signX, signZ, EdgeYaw(hasX, signX, signZ)));
            else
                parts.Add(OnQuadrant(BlockPartKind.OuterCorner, center, signX, signZ, CornerYaw(signX, signZ)));
        }

        private static BlockPart OnVertex(Vector3 cellCenter, int signX, int signZ)
        {
            var vertex = cellCenter + new Vector3(signX * HalfCell, 0f, signZ * HalfCell);

            return new BlockPart(BlockPartKind.InnerCorner, new Placement(vertex, CornerYaw(signX, signZ)));
        }

        private static BlockPart OnQuadrant(BlockPartKind kind, Vector3 cellCenter, int signX, int signZ, float yaw)
        {
            var quadrant = new Vector3(signX * Quadrant, 0f, signZ * Quadrant);
            Vector3 pivotOffset = quadrant - Quaternion.Euler(0f, yaw, 0f) * Footprint;

            return new BlockPart(kind, new Placement(cellCenter + pivotOffset, yaw));
        }

        /// <summary>The Edge's wall faces the side that has no neighbor.</summary>
        private static float EdgeYaw(bool hasX, int signX, int signZ)
        {
            if (!hasX)
                return PieceYaw.Facing(signX > 0 ? Direction.Right : Direction.Left);

            return PieceYaw.Facing(signZ > 0 ? Direction.Up : Direction.Down);
        }

        /// <summary>Block corners at yaw 0 round toward (+X, +Z); InnerCorner's empty quadrant is (+X, +Z).</summary>
        private static float CornerYaw(int signX, int signZ)
        {
            if (signX > 0)
                return signZ > 0 ? 0f : 90f;

            return signZ < 0 ? 180f : 270f;
        }
    }
}
