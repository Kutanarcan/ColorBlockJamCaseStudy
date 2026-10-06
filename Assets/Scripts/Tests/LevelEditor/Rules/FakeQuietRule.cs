using System.Collections.Generic;
using Game.LevelEditor;

namespace Game.Tests.LevelEditor
{
    /// <summary>
    /// Test-only rule outside the editor assembly that never reports. Proves discovery: a new rule is a new file.
    /// It must stay quiet, because discovery also finds it while the real editor runs.
    /// </summary>
    public sealed class FakeQuietRule : ILevelRule
    {
        public void Check(LevelModel level, List<RuleViolation> violations)
        {
        }
    }
}
