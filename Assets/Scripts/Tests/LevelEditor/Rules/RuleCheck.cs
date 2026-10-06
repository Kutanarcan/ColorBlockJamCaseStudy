using System.Collections.Generic;
using Game.LevelEditor;

namespace Game.Tests.LevelEditor
{
    /// <summary>Test-only: runs one rule and returns what it reported.</summary>
    internal static class RuleCheck
    {
        public static List<RuleViolation> Run(ILevelRule rule, LevelModel model)
        {
            var violations = new List<RuleViolation>();
            rule.Check(model, violations);

            return violations;
        }
    }
}
