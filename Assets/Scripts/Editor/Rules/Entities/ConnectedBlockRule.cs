using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>Every block shape is connected through its sides (no diagonal-only links).</summary>
    public sealed class ConnectedBlockRule : ILevelRule
    {
        private static readonly Cell[] Neighbours = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };

        private readonly List<int> blocks = new List<int>();
        private readonly HashSet<int> reached = new HashSet<int>();
        private readonly Stack<int> open = new Stack<int>();

        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            level.CollectEntities(EntityKind.Block, blocks);

            foreach (int block in blocks)
            {
                if (CountReachable(level, block) != level.CellsOf(block).Count)
                {
                    int first = level.CellsOf(block)[0];
                    violations.Add(new RuleViolation($"Block at {level.CellOf(first)} is not one connected shape.", first));
                }
            }
        }

        private int CountReachable(LevelModel level, int block)
        {
            reached.Clear();
            open.Clear();

            int start = level.CellsOf(block)[0];
            reached.Add(start);
            open.Push(start);

            while (open.Count > 0)
            {
                Cell cell = level.CellOf(open.Pop());

                foreach (Cell offset in Neighbours)
                {
                    Cell next = cell + offset;

                    if (level.Contains(next) && level.OwnerOf(next) == block && reached.Add(level.IndexOf(next)))
                        open.Push(level.IndexOf(next));
                }
            }

            return reached.Count;
        }
    }
}
