using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class RuleReportTests
    {
        [Test]
        public void Refresh_FlagsTheCellsViolationsPointAt()
        {
            LevelModel model = AsciiModel.Parse(
                    "###",
                    "#.#",
                    "#.#")
                .Build();
            var report = new RuleReport(new LevelRules(new EdgeCellsRule(), new FakeReportingRule("whole level")));

            report.Refresh(model);

            Assert.That(report.Violations.Count, Is.EqualTo(2));
            Assert.That(report.IsFlagged(model.IndexOf(new Cell(1, 0))), Is.True);
            Assert.That(report.IsFlagged(model.IndexOf(new Cell(1, 1))), Is.False);
        }
    }
}
