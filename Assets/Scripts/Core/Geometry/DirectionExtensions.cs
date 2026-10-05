using System;

namespace Game.Core
{
    public static class DirectionExtensions
    {
        public static Cell ToOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return new Cell(0, 1);
                case Direction.Down: return new Cell(0, -1);
                case Direction.Left: return new Cell(-1, 0);
                case Direction.Right: return new Cell(1, 0);
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
