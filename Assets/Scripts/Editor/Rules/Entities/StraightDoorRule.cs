using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>A door entity is one unbroken straight line, perpendicular to its direction.</summary>
    public sealed class StraightDoorRule : ILevelRule
    {
        private readonly List<int> doors = new List<int>();

        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            level.CollectEntities(EntityKind.Door, doors);

            foreach (int door in doors)
            {
                if (!IsStraight(level, door))
                {
                    int first = level.CellsOf(door)[0];
                    violations.Add(new RuleViolation(
                        $"Door at {level.CellOf(first)} must be one unbroken line across its direction.", first));
                }
            }
        }

        private static bool IsStraight(LevelModel level, int door)
        {
            bool facesUpOrDown = level.DirectionOf(door) == Direction.Up || level.DirectionOf(door) == Direction.Down;
            IReadOnlyList<int> cells = level.CellsOf(door);
            Cell first = level.CellOf(cells[0]);
            int line = facesUpOrDown ? first.Y : first.X;
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (int index in cells)
            {
                Cell cell = level.CellOf(index);

                if ((facesUpOrDown ? cell.Y : cell.X) != line)
                    return false;

                int along = facesUpOrDown ? cell.X : cell.Y;
                if (along < min) min = along;
                if (along > max) max = along;
            }

            return max - min + 1 == cells.Count;
        }
    }
}
