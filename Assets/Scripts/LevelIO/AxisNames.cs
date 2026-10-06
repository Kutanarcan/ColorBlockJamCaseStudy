using System;
using Game.Core;

namespace Game.LevelIO
{
    /// <summary>Axis to and from its saved name, without Enum reflection.</summary>
    internal static class AxisNames
    {
        public static string ToName(Axis axis)
        {
            switch (axis)
            {
                case Axis.Horizontal: return "Horizontal";
                case Axis.Vertical: return "Vertical";
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public static bool TryParse(string name, out Axis axis)
        {
            switch (name)
            {
                case "Horizontal": axis = Axis.Horizontal; return true;
                case "Vertical": axis = Axis.Vertical; return true;
                default: axis = default; return false;
            }
        }
    }
}
