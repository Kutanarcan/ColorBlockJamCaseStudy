using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelWideRulesTests
    {
        [TestCase(0f)]
        [TestCase(-5f)]
        public void TimeLimit_AtOrBelowZero_Violates(float seconds)
        {
            var model = new LevelModel(3, 3) { TimeLimit = seconds };

            Assert.That(RuleCheck.Run(new TimeLimitRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void TimeLimit_AboveZero_Passes()
        {
            var model = new LevelModel(3, 3) { TimeLimit = 0.5f };

            Assert.That(RuleCheck.Run(new TimeLimitRule(), model), Is.Empty);
        }

        [Test]
        public void PaletteColor_PastThePalette_Violates_OnTheEntitysCell()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 3)
                .Door('1', 2, Direction.Down)
                .Build();

            var violations = RuleCheck.Run(new PaletteColorRule(3), model);

            Assert.That(violations.Count, Is.EqualTo(1), "the block's 3 is past a 3-color palette; the door's 2 is in");
            Assert.That(violations[0].CellIndex, Is.EqualTo(model.IndexOf(new Cell(1, 1))));
        }

        [Test]
        public void InvalidModifierValue_IsReported_ThroughCoreValidation()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();
            model.AddModifier(model.OwnerOf(new Cell(1, 1)), new IceData { Count = 0 });

            Assert.That(RuleCheck.Run(new CoreValidationRule(), model).Count, Is.EqualTo(1));
        }
    }
}
