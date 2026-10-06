using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelResizerTests
    {
        // 5 x 4: block 'A' at (1, 2), a door facing down at (2, 0).
        private static LevelModel Level() =>
            AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "#...#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

        private static bool EdgeRulesPass(LevelModel model) =>
            RuleCheck.Run(new EdgeCellsRule(), model).Count == 0 && RuleCheck.Run(new EdgeDoorsRule(), model).Count == 0;

        [Test]
        public void GrowTop_MovesTheTopFrame_AndKeepsTheInside()
        {
            LevelModel level = Level();
            int block = level.OwnerOf(new Cell(1, 2));

            LevelModel grown = LevelResizer.Resize(level, Direction.Up, 1);

            Assert.That(grown.Height, Is.EqualTo(5));
            Assert.That(grown.OwnerOf(new Cell(1, 2)), Is.EqualTo(block));
            Assert.That(grown.OwnerOf(new Cell(2, 3)), Is.EqualTo(LevelModel.None));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(2, 0))), Is.EqualTo(EntityKind.Door));
            Assert.That(EdgeRulesPass(grown), Is.True);
        }

        [Test]
        public void GrowBottom_TakesTheBottomDoorAlong()
        {
            LevelModel grown = LevelResizer.Resize(Level(), Direction.Down, 1);

            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(2, 0))), Is.EqualTo(EntityKind.Door));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(1, 3))), Is.EqualTo(EntityKind.Block));
            Assert.That(EdgeRulesPass(grown), Is.True);
        }

        [Test]
        public void GrowLeft_ClosesTheNewColumnsEnds()
        {
            LevelModel grown = LevelResizer.Resize(Level(), Direction.Left, 1);

            Assert.That(grown.Width, Is.EqualTo(6));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(2, 2))), Is.EqualTo(EntityKind.Block));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(1, 0))), Is.EqualTo(EntityKind.Wall));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(1, 3))), Is.EqualTo(EntityKind.Wall));
            Assert.That(EdgeRulesPass(grown), Is.True);
        }

        [Test]
        public void Grow_GivesANewEndCellToAWall_NotToASideDoor()
        {
            LevelModel level = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "1...#",
                    "#####")
                .Block('A', 0)
                .Door('1', 0, Direction.Left)
                .Build();
            int door = level.OwnerOf(new Cell(0, 1));

            LevelModel grown = LevelResizer.Resize(level, Direction.Down, 1);

            Assert.That(grown.OwnerOf(new Cell(0, 2)), Is.EqualTo(door));
            Assert.That(grown.KindOf(grown.OwnerOf(new Cell(0, 1))), Is.EqualTo(EntityKind.Wall));
            Assert.That(grown.CellsOf(door).Count, Is.EqualTo(1));
        }

        [Test]
        public void ShrinkTop_RemovesTheLineInsideTheFrame()
        {
            LevelModel level = Level();
            int block = level.OwnerOf(new Cell(1, 2));

            Assert.That(LevelResizer.CutCount(level, Direction.Up), Is.EqualTo(1));

            LevelModel shrunk = LevelResizer.Resize(level, Direction.Up, -1);

            Assert.That(shrunk.Height, Is.EqualTo(3));
            Assert.That(shrunk.Exists(block), Is.False);
            Assert.That(EdgeRulesPass(shrunk), Is.True);
        }

        [Test]
        public void CutCount_IgnoresTheFrameWallsAtTheLineEnds()
        {
            Assert.That(LevelResizer.CutCount(Level(), Direction.Down), Is.EqualTo(0));
        }

        [Test]
        public void Resize_LeavesTheGivenLevelAsItWas()
        {
            LevelModel level = Level();

            LevelResizer.Resize(level, Direction.Right, 1);

            Assert.That(level.Width, Is.EqualTo(5));
            Assert.That(level.KindOf(level.OwnerOf(new Cell(4, 2))), Is.EqualTo(EntityKind.Wall));
        }

        [Test]
        public void CanResize_KeepsTheSizeWithinTheEditorLimits()
        {
            var smallest = new LevelModel(LevelTemplates.MinSize, LevelTemplates.MinSize);
            var largest = new LevelModel(LevelTemplates.MaxSize, LevelTemplates.MaxSize);

            Assert.That(LevelResizer.CanResize(smallest, Direction.Up, -1), Is.False);
            Assert.That(LevelResizer.CanResize(smallest, Direction.Left, 1), Is.True);
            Assert.That(LevelResizer.CanResize(largest, Direction.Right, 1), Is.False);
            Assert.That(LevelResizer.CanResize(largest, Direction.Down, 2), Is.False);
        }
    }
}
