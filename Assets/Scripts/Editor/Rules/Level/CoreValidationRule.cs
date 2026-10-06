using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// Runs Core's load-time validation on the level being edited, so the editor never saves what
    /// LevelSession.TryCreate would reject. In practice this reports modifier values (e.g. Ice count).
    /// </summary>
    public sealed class CoreValidationRule : ILevelRule
    {
        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            foreach (LevelError error in LevelValidator.Validate(LevelModelConverter.ToLevelData(level)))
            {
                violations.Add(new RuleViolation(error.ToString()));
            }
        }
    }
}
