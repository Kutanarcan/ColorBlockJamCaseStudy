using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ColorsTests
    {
        private static readonly Cell ACell = new Cell(2, 1);

        // A (base color 0) sits above a door of the given color.
        private static Board NewBoard(int doorColor, params ModifierData[] aModifiers) =>
            BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#.A.#",
                    "##1##")
                .Block('A', 0, aModifiers)
                .Door('1', doorColor, Direction.Down)
                .Build());

        private static ModifierData ColorSource(int colorId) =>
            new FakeModifierData(() => new FakeColorSource(colorId));

        [Test]
        public void Of_IsTheBaseColor_WithoutAColorSource()
        {
            Board board = NewBoard(0);

            Assert.That(Colors.Of(board.EntityAt(ACell)), Is.EqualTo(0));
        }

        [Test]
        public void Of_IsTheLatestColorSource()
        {
            Board board = NewBoard(0, ColorSource(3), ColorSource(4));

            Assert.That(Colors.Of(board.EntityAt(ACell)), Is.EqualTo(4));
        }

        [TestCase(1, MoveResult.Exited)]  // door matches the overridden color
        [TestCase(0, MoveResult.Blocked)] // door matches only the base color
        public void ColorSource_DecidesWhichDoorTheBlockExitsThrough(int doorColor, MoveResult expected)
        {
            Board board = NewBoard(doorColor, ColorSource(1));
            var a = (Block)board.EntityAt(ACell);

            Assert.That(new BlockMover(board).TryMove(a, Direction.Down), Is.EqualTo(expected));
        }
    }
}
