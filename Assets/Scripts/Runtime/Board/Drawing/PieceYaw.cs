using System;
using Game.Core;

namespace Game.Runtime
{
    /// <summary>
    /// Y rotations of the wall kit (FINDINGS board edge table). A Wall or Door at yaw 0 faces +Z (its open
    /// side looks up the board); a Corner at yaw 0 is rounded toward (−X, −Z).
    /// </summary>
    public static class PieceYaw
    {
        public static float Facing(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return 0f;
                case Direction.Right: return 90f;
                case Direction.Down: return 180f;
                case Direction.Left: return 270f;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>The corner whose round side points toward (signX, signZ), each −1 or +1.</summary>
        public static float CornerRoundedToward(int signX, int signZ)
        {
            if (signX < 0)
                return signZ < 0 ? 0f : 90f;

            return signZ > 0 ? 180f : 270f;
        }
    }
}
