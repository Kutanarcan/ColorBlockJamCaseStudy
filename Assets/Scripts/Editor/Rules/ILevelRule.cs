using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>
    /// One design rule the editor checks before saving. Implementations are discovered (LevelRules.Discover),
    /// so a new rule is a new file. Needs a parameterless constructor.
    /// </summary>
    public interface ILevelRule
    {
        void Check(LevelModel level, List<RuleViolation> violations);
    }
}
