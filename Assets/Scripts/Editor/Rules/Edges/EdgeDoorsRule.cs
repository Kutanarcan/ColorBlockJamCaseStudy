using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>A door on the edge points out of the grid; a corner cell is never a door.</summary>
    public sealed class EdgeDoorsRule : ILevelRule
    {
        private readonly List<int> doors = new List<int>();

        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            level.CollectEntities(EntityKind.Door, doors);

            foreach (int door in doors)
            {
                foreach (int index in level.CellsOf(door))
                {
                    CheckCell(level, door, index, violations);
                }
            }
        }

        private static void CheckCell(LevelModel level, int door, int index, List<RuleViolation> violations)
        {
            Cell cell = level.CellOf(index);

            if (!EdgeCells.IsEdge(level, cell))
                return;

            if (EdgeCells.IsCorner(level, cell))
            {
                violations.Add(new RuleViolation($"Corner cell {cell} cannot be a door.", index));

                return;
            }

            Direction outward = EdgeCells.Outward(level, cell);

            if (level.DirectionOf(door) != outward)
                violations.Add(new RuleViolation($"Door at {cell} is on the edge and must point {outward}.", index));
        }
    }
}
