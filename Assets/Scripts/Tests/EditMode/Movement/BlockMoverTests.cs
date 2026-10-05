using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class BlockMoverTests
    {
        [TestCase(Direction.Up)]    // block
        [TestCase(Direction.Left)]  // door of another color
        [TestCase(Direction.Right)] // wall
        [TestCase(Direction.Down)]  // wall
        public void TryMove_StopsAt_OccupiedCell(Direction direction)
        {
            // A is boxed in: B above, a door of another color on the left, walls right and below.
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#.B.#",
                    "#1A##",
                    "#####")
                .Block('A', 0)
                .Block('B', 2)
                .Door('1', 1, Direction.Left)
                .Build());
            var a = (Block)board.EntityAt(new Cell(2, 1));
            Entity neighbor = board.EntityAt(new Cell(2, 1) + direction.ToOffset());

            MoveResult result = new BlockMover(board).TryMove(a, direction);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.Position, Is.EqualTo(new Cell(2, 1)));
            Assert.That(board.EntityAt(new Cell(2, 1)), Is.SameAs(a));
            Assert.That(board.EntityAt(new Cell(2, 1) + direction.ToOffset()), Is.SameAs(neighbor));
        }

        [Test]
        public void TryMove_IntoEmptyCells_MovesAndUpdatesOccupancy()
        {
            Board board = BarLevel();
            var a = (Block)board.EntityAt(new Cell(1, 2));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Moved));
            Assert.That(a.Position, Is.EqualTo(new Cell(1, 1)));
            Assert.That(board.EntityAt(new Cell(1, 1)), Is.SameAs(a));
            Assert.That(board.EntityAt(new Cell(2, 1)), Is.SameAs(a));
            Assert.That(board.EntityAt(new Cell(1, 2)), Is.Null);
            Assert.That(board.EntityAt(new Cell(2, 2)), Is.Null);
        }

        [Test]
        public void TryMove_AlongOwnLength_TreatsOwnCellsAsFree()
        {
            Board board = BarLevel();
            var a = (Block)board.EntityAt(new Cell(1, 2));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Right);

            Assert.That(result, Is.EqualTo(MoveResult.Moved));
            Assert.That(board.EntityAt(new Cell(1, 2)), Is.Null);
            Assert.That(board.EntityAt(new Cell(2, 2)), Is.SameAs(a));
            Assert.That(board.EntityAt(new Cell(3, 2)), Is.SameAs(a));
        }

        private static Board BarLevel() =>
            BoardBuilder.Build(AsciiLevel.Parse(
                    "######",
                    "#AA..#",
                    "#....#",
                    "######")
                .Block('A', 0)
                .Build());
    }
}
