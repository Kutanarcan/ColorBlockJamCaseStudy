using System;
using UnityEngine;

namespace Game.Prototype
{
    // Where can the held block go? Logic only; asks Board for the rules, never touches transforms.
    [Serializable]
    public class DragCollision
    {
        [SerializeField] int maxStepsPerFrame = 8; // cap on cells walked in one frame

        Board board;

        public void Init(Board board) => this.board = board;

        // Walk the block toward target one cell at a time.
        // Larger remaining axis first; if it is blocked, try the other axis; stop when both are blocked.
        public void Walk(int id, Vector2Int target)
        {
            for (int i = 0; i < maxStepsPerFrame; i++)
            {
                var rem = target - board.BlockOffset(id);
                if (rem == Vector2Int.zero) return;

                var xStep = new Vector2Int(Math.Sign(rem.x), 0);
                var yStep = new Vector2Int(0, Math.Sign(rem.y));
                bool xFirst = Mathf.Abs(rem.x) >= Mathf.Abs(rem.y);
                var first = xFirst ? xStep : yStep;
                var second = xFirst ? yStep : xStep;

                if (first != Vector2Int.zero && board.TryStep(id, first)) continue;
                if (second != Vector2Int.zero && board.TryStep(id, second)) continue;
                return;
            }
        }

        // How far (max half a cell per axis) the visual may lean from its cell toward the pointer.
        // pointer and the result are in cells, relative to the board origin.
        // A blocked direction gives 0 on that axis; a blocked diagonal drops the smaller axis.
        public Vector2 Lean(int id, Vector2 pointer)
        {
            var cell = board.BlockOffset(id);
            var f = pointer - cell;

            f.x = Mathf.Clamp(f.x, -0.5f, 0.5f);
            f.y = Mathf.Clamp(f.y, -0.5f, 0.5f);

            int sx = Math.Sign(f.x);
            int sy = Math.Sign(f.y);
            if (sx != 0 && !board.CanPlace(id, new Vector2Int(sx, 0))) { f.x = 0f; sx = 0; }
            if (sy != 0 && !board.CanPlace(id, new Vector2Int(0, sy))) { f.y = 0f; sy = 0; }

            if (sx != 0 && sy != 0 && !board.CanPlace(id, new Vector2Int(sx, sy)))
            {
                if (Mathf.Abs(f.x) < Mathf.Abs(f.y)) f.x = 0f;
                else f.y = 0f;
            }

            return cell + f;
        }
    }
}
