using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelFrameTests
    {
        [Test]
        public void Seal_TurnsEmptyEdgeCellsIntoTheFrameWall_AndLeavesTheInside()
        {
            LevelModel level = AsciiModel.Parse(
                    "##.#",
                    "#..#",
                    "####")
                .Build();
            int wall = level.OwnerOf(new Cell(0, 0));

            LevelFrame.Seal(level);

            Assert.That(level.OwnerOf(new Cell(2, 2)), Is.EqualTo(wall));
            Assert.That(level.OwnerOf(new Cell(1, 1)), Is.EqualTo(LevelModel.None));
        }

        [Test]
        public void Seal_WithNoWallAtAll_AddsOne()
        {
            var level = new LevelModel(3, 3);

            LevelFrame.Seal(level);

            Assert.That(level.CountOf(EntityKind.Wall), Is.EqualTo(1));
            Assert.That(RuleCheck.Run(new EdgeCellsRule(), level), Is.Empty);
        }

        [Test]
        public void Allows_BlocksOnlyInside()
        {
            var level = new LevelModel(3, 3);

            Assert.That(LevelFrame.Allows(level, new Cell(0, 1), EntityKind.Block), Is.False);
            Assert.That(LevelFrame.Allows(level, new Cell(0, 1), EntityKind.Door), Is.True);
            Assert.That(LevelFrame.Allows(level, new Cell(1, 1), EntityKind.Block), Is.True);
        }
    }
}
