using System.Linq;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelRulesTests
    {
        [Test]
        public void Discover_FindsBuiltInRules_AndRulesFromOtherAssemblies()
        {
            var types = LevelRules.Discover().Rules.Select(rule => rule.GetType()).ToArray();

            Assert.That(types, Has.Member(typeof(EdgeCellsRule)));
            Assert.That(types, Has.Member(typeof(DoorWidthRule)));
            Assert.That(types, Has.Member(typeof(CoreValidationRule)));
            Assert.That(types, Has.Member(typeof(FakeQuietRule)));
        }

        [Test]
        public void Discover_Skips_RulesWithoutAParameterlessConstructor()
        {
            var types = LevelRules.Discover().Rules.Select(rule => rule.GetType()).ToArray();

            Assert.That(types, Has.No.Member(typeof(FakeReportingRule)));
        }

        [Test]
        public void Check_CollectsViolations_FromEveryRule()
        {
            var rules = new LevelRules(new FakeReportingRule("first"), new FakeQuietRule(), new FakeReportingRule("second"));

            var messages = rules.Check(new LevelModel(3, 3)).Select(v => v.Message).ToArray();

            Assert.That(messages, Is.EqualTo(new[] { "first", "second" }));
        }
    }
}
