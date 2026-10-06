using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>The latest rule results for the open level: the violations and the cells they point at.</summary>
    public sealed class RuleReport
    {
        private readonly LevelRules rules;
        private readonly HashSet<int> flaggedCells = new HashSet<int>();

        public IReadOnlyList<RuleViolation> Violations { get; private set; } = new List<RuleViolation>();

        public RuleReport(LevelRules rules) => this.rules = rules;

        public bool IsFlagged(int cellIndex) => flaggedCells.Contains(cellIndex);

        public void Refresh(LevelModel level)
        {
            List<RuleViolation> found = rules.Check(level);
            flaggedCells.Clear();

            foreach (RuleViolation violation in found)
            {
                if (violation.CellIndex != LevelModel.None)
                    flaggedCells.Add(violation.CellIndex);
            }

            Violations = found;
        }
    }
}
