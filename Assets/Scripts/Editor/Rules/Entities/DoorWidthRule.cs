using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// Every block has, in some direction, a run of doors of its color at least as wide as the block across that
    /// direction. Runs may span several door entities on the same line (D7). Modifiers such as Arrow are not
    /// considered: this is a necessary condition, not a solver.
    /// </summary>
    public sealed class DoorWidthRule : ILevelRule
    {
        private static readonly Direction[] Directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        private readonly List<int> blocks = new List<int>();
        private readonly List<int> doors = new List<int>();

        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            level.CollectEntities(EntityKind.Block, blocks);
            level.CollectEntities(EntityKind.Door, doors);

            foreach (int block in blocks)
            {
                if (!HasWideEnoughDoor(level, block))
                {
                    int first = level.CellsOf(block)[0];
                    violations.Add(new RuleViolation(
                        $"Block at {level.CellOf(first)} has no door of its color wide enough to exit.", first));
                }
            }
        }

        private bool HasWideEnoughDoor(LevelModel level, int block)
        {
            foreach (Direction direction in Directions)
            {
                if (LongestDoorRun(level, level.ColorOf(block), direction) >= Width(level, block, direction))
                    return true;
            }

            return false;
        }

        /// <summary>The block's extent across the direction: its columns for Up/Down, its rows for Left/Right.</summary>
        private static int Width(LevelModel level, int block, Direction direction)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (int index in level.CellsOf(block))
            {
                int along = Across(level.CellOf(index), direction);
                if (along < min) min = along;
                if (along > max) max = along;
            }

            return max - min + 1;
        }

        private int LongestDoorRun(LevelModel level, int colorId, Direction direction)
        {
            var linesToCells = new Dictionary<int, List<int>>();

            foreach (int door in doors)
            {
                if (level.ColorOf(door) != colorId || level.DirectionOf(door) != direction)
                    continue;

                foreach (int index in level.CellsOf(door))
                {
                    Cell cell = level.CellOf(index);
                    int line = direction == Direction.Up || direction == Direction.Down ? cell.Y : cell.X;

                    if (!linesToCells.TryGetValue(line, out List<int> along))
                        linesToCells.Add(line, along = new List<int>());

                    along.Add(Across(cell, direction));
                }
            }

            int longest = 0;

            foreach (List<int> along in linesToCells.Values)
            {
                int run = LongestRun(along);
                if (run > longest) longest = run;
            }

            return longest;
        }

        private static int LongestRun(List<int> values)
        {
            values.Sort();
            int longest = 1;
            int current = 1;

            for (int i = 1; i < values.Count; i++)
            {
                current = values[i] == values[i - 1] + 1 ? current + 1 : 1;
                if (current > longest) longest = current;
            }

            return longest;
        }

        private static int Across(Cell cell, Direction direction) =>
            direction == Direction.Up || direction == Direction.Down ? cell.X : cell.Y;
    }
}
