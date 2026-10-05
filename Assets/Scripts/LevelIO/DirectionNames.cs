using System;
using Game.Core;

namespace Game.LevelIO
{
    /// <summary>Direction to and from its saved name, without Enum reflection.</summary>
    internal static class DirectionNames
    {
        public static string ToName(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return "Up";
                case Direction.Down: return "Down";
                case Direction.Left: return "Left";
                case Direction.Right: return "Right";
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static bool TryParse(string name, out Direction direction)
        {
            switch (name)
            {
                case "Up": direction = Direction.Up; return true;
                case "Down": direction = Direction.Down; return true;
                case "Left": direction = Direction.Left; return true;
                case "Right": direction = Direction.Right; return true;
                default: direction = default; return false;
            }
        }
    }
}
