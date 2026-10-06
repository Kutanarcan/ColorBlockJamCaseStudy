using System.Collections.Generic;
using Game.LevelEditor;

namespace Game.Tests.LevelEditor
{
    /// <summary>
    /// Test-only rule that always reports. Its constructor takes an argument, so discovery skips it and it never
    /// blocks a save in the real editor.
    /// </summary>
    public sealed class FakeReportingRule : ILevelRule
    {
        private readonly string message;

        public FakeReportingRule(string message) => this.message = message;

        public void Check(LevelModel level, List<RuleViolation> violations) => violations.Add(new RuleViolation(message));
    }
}
