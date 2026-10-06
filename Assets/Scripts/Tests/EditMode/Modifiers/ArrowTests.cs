using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ArrowTests
    {
        // A is locked to the horizontal axis, has free cells left, right and above, and sits above a matching door.
        private static Board NewBoard() =>
            BoardBuilder.Build(AsciiLevel.Parse(
                    "##.##",
                    "#.A.#",
                    "##1##")
                .Block('A', 0, new ArrowData { Axis = Axis.Horizontal })
                .Door('1', 0, Direction.Down)
                .Build());

        [TestCase(Direction.Left)]
        [TestCase(Direction.Right)]
        public void Arrow_AllowsBothWaysAlongItsAxis(Direction direction)
        {
            Board board = NewBoard();
            var a = (Block)board.EntityAt(new Cell(2, 1));

            Assert.That(new BlockMover(board).TryMove(a, direction), Is.EqualTo(MoveResult.Moved));
        }

        [TestCase(Direction.Up)]   // free cell, vetoed
        [TestCase(Direction.Down)] // matching door, exit vetoed
        public void Arrow_VetoesTheOtherAxis(Direction direction)
        {
            Board board = NewBoard();
            var a = (Block)board.EntityAt(new Cell(2, 1));

            MoveResult result = new BlockMover(board).TryMove(a, direction);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
            Assert.That(a.Position, Is.EqualTo(new Cell(2, 1)));
        }
    }
}
