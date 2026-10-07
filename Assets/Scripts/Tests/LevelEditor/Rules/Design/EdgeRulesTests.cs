using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class EdgeRulesTests
    {
        private static LevelModel ValidLevel() =>
            AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

        [Test]
        public void ValidLevel_PassesBothEdgeRules()
        {
            Assert.That(RuleCheck.Run(new EdgeCellsRule(), ValidLevel()), Is.Empty);
            Assert.That(RuleCheck.Run(new EdgeDoorsRule(), ValidLevel()), Is.Empty);
        }

        [Test]
        public void EmptyEdgeCell_Violates()
        {
            LevelModel model = ValidLevel();
            model.Erase(new Cell(2, 2));

            var violations = RuleCheck.Run(new EdgeCellsRule(), model);

            Assert.That(violations.Count, Is.EqualTo(1));
            Assert.That(violations[0].CellIndex, Is.EqualTo(model.IndexOf(new Cell(2, 2))));
        }

        [Test]
        public void BlockOnTheEdge_Violates()
        {
            LevelModel model = AsciiModel.Parse(
                    "#A###",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new EdgeCellsRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void EdgeDoorPointingInward_Violates()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Up)
                .Build();

            Assert.That(RuleCheck.Run(new EdgeDoorsRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void CornerDoor_Violates()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "1####")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new EdgeDoorsRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void InnerDoor_IsNotAnEdgeConcern()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "#.1.#",
                    "#####")
                .Block('A', 0)
                .Door('1', 0, Direction.Up)
                .Build();

            Assert.That(RuleCheck.Run(new EdgeDoorsRule(), model), Is.Empty);
        }
    }
}
