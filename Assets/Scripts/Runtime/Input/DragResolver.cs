using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Resolves where a dragged block goes (FINDINGS: free 2D drag, logic cell by cell). The logic moves toward the
    /// pointer through <see cref="LevelSession.TryMove"/>; the visual follows it, clamped to reachable space.
    /// Positions are block positions in cell units: a whole number is the block standing on that position.
    /// </summary>
    public sealed class DragResolver
    {
        private const int MaxStepsPerFrame = 8;
        private const float MaxVisualOffset = 0.5f;

        private readonly LevelSession session;
        private Cell lastMoveOffset;

        public DragResolver(LevelSession session) => this.session = session;

        /// <summary>A new drag: forgets the last move of the previous one.</summary>
        public void BeginDrag() => lastMoveOffset = default;

        /// <summary>
        /// Moves one cell at a time toward <paramref name="target"/>: the larger remaining axis first, the other if
        /// it is blocked, until both are blocked or the target is reached. Exited ends the moves; the block is gone.
        /// </summary>
        public MoveResult MoveToward(Block block, Cell target)
        {
            MoveResult moved = MoveResult.Blocked;

            for (int i = 0; i < MaxStepsPerFrame && block.Position != target; i++)
            {
                MoveResult step = TryStep(block, target - block.Position);

                if (step != MoveResult.Moved)
                    return step == MoveResult.Exited ? step : moved;

                moved = MoveResult.Moved;
            }

            return moved;
        }

        /// <summary>
        /// The block's position plus at most half a cell per axis toward <paramref name="pointer"/>. A refused
        /// direction gets no offset; a refused diagonal drops the smaller axis. The cell the last move left is never
        /// refused while it is free: the block was just there, so a one-way block follows the pointer between its
        /// cells instead of jumping a whole cell.
        /// </summary>
        public Vector2 ClampToReachable(Block block, Vector2 pointer)
        {
            var position = new Vector2(block.Position.X, block.Position.Y);
            float x = Mathf.Clamp(pointer.x - position.x, -MaxVisualOffset, MaxVisualOffset);
            float y = Mathf.Clamp(pointer.y - position.y, -MaxVisualOffset, MaxVisualOffset);

            if (x != 0f && !CanOffsetToward(block, true, Math.Sign(x)))
                x = 0f;

            if (y != 0f && !CanOffsetToward(block, false, Math.Sign(y)))
                y = 0f;

            if (x != 0f && y != 0f && !session.Board.CanPlace(block, new Cell(Math.Sign(x), Math.Sign(y))))
            {
                if (Mathf.Abs(x) < Mathf.Abs(y))
                    x = 0f;
                else
                    y = 0f;
            }

            return position + new Vector2(x, y);
        }

        private MoveResult TryStep(Block block, Cell remaining)
        {
            bool xFirst = Math.Abs(remaining.X) >= Math.Abs(remaining.Y);
            MoveResult first = TryMoveAlong(block, remaining, xFirst);

            return first != MoveResult.Blocked ? first : TryMoveAlong(block, remaining, !xFirst);
        }

        private MoveResult TryMoveAlong(Block block, Cell remaining, bool alongX)
        {
            int sign = Math.Sign(alongX ? remaining.X : remaining.Y);

            if (sign == 0)
                return MoveResult.Blocked;

            MoveResult result = session.TryMove(block, ToDirection(alongX, sign));

            if (result == MoveResult.Moved)
                lastMoveOffset = Offset(alongX, sign);

            return result;
        }

        private bool CanOffsetToward(Block block, bool alongX, int sign)
        {
            Cell offset = Offset(alongX, sign);
            bool towardPreviousCell = offset + lastMoveOffset == default;

            return session.Board.CanPlace(block, offset)
                   && (towardPreviousCell || Capabilities.CanMove(block, ToDirection(alongX, sign)));
        }

        private static Cell Offset(bool alongX, int sign) => alongX ? new Cell(sign, 0) : new Cell(0, sign);

        private static Direction ToDirection(bool alongX, int sign)
        {
            if (alongX)
                return sign > 0 ? Direction.Right : Direction.Left;

            return sign > 0 ? Direction.Up : Direction.Down;
        }
    }
}
