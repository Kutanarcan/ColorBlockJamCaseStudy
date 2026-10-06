using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>The time limit is above zero: a level without time fails the moment it starts.</summary>
    public sealed class TimeLimitRule : ILevelRule
    {
        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            if (level.TimeLimit <= 0f)
                violations.Add(new RuleViolation($"Time limit must be above zero (it is {level.TimeLimit})."));
        }
    }
}
