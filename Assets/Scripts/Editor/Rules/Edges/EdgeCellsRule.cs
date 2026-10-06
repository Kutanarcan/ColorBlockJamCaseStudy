using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// Edge cells are walls or doors, never empty and never a block. This is the rule that lets Core skip
    /// bounds checks (D3).
    /// </summary>
    public sealed class EdgeCellsRule : ILevelRule
    {
        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    var cell = new Cell(x, y);

                    if (!EdgeCells.IsEdge(level, cell))
                        continue;

                    int owner = level.OwnerOf(cell);

                    if (owner == LevelModel.None || level.KindOf(owner) == EntityKind.Block)
                        violations.Add(new RuleViolation($"Edge cell {cell} must be a wall or a door.", level.IndexOf(cell)));
                }
            }
        }
    }
}
