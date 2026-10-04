using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    public enum PieceKind { OuterCorner, Edge, Center }

    public struct Piece
    {
        public PieceKind kind;
        public Vector3 localPos;
        public float yRot;
    }

    // At rotation 0 every piece covers the top-right quadrant (+X, +Z) of its pivot (Q6).
    // OuterCorner rounds toward (+X, +Z); Edge's wall faces +Z; Center is flat.
    public static class BlockDrawRule
    {
        static readonly Vector2Int[] Quadrants =
        {
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1),
        };

        public static List<Piece> Build(List<Vector2Int> cells, float cellSize)
        {
            var filled = new HashSet<Vector2Int>(cells);
            var pieces = new List<Piece>();

            // Pass 2 (quadrants). Pass 1 (InnerCorner) comes in Phase 4.
            foreach (var cell in cells)
            {
                var cellCenter = new Vector3(cell.x * cellSize + cellSize * 0.5f, 0f, cell.y * cellSize + cellSize * 0.5f);

                foreach (var q in Quadrants)
                {
                    bool hasX = filled.Contains(cell + new Vector2Int(q.x, 0));
                    bool hasZ = filled.Contains(cell + new Vector2Int(0, q.y));

                    PieceKind kind;
                    float rot;
                    if (!hasX && !hasZ) { kind = PieceKind.OuterCorner; rot = CornerRotation(q); }
                    else if (hasX && hasZ) { kind = PieceKind.Center; rot = 0f; }
                    else { kind = PieceKind.Edge; rot = WallRotation(!hasX ? new Vector2Int(q.x, 0) : new Vector2Int(0, q.y)); }

                    pieces.Add(new Piece { kind = kind, yRot = rot, localPos = cellCenter + PivotOffset(q, rot) });
                }
            }
            return pieces;
        }

        // Shift the pivot so the rotated top-right footprint lands on quadrant q.
        static Vector3 PivotOffset(Vector2Int q, float rot)
        {
            var target = new Vector3(q.x * 0.5f, 0f, q.y * 0.5f);
            var footprint = Quaternion.Euler(0f, rot, 0f) * new Vector3(0.5f, 0f, 0.5f);
            return target - footprint;
        }

        // Unity Y rotation is clockwise from above: +Z -> +X -> -Z -> -X.
        static float WallRotation(Vector2Int dir)
        {
            if (dir.y > 0) return 0f;
            if (dir.x > 0) return 90f;
            if (dir.y < 0) return 180f;
            return 270f;
        }

        static float CornerRotation(Vector2Int q)
        {
            if (q.x > 0 && q.y > 0) return 0f;
            if (q.x > 0) return 90f;
            if (q.y < 0) return 180f;
            return 270f;
        }
    }
}
