using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ShapeMoveTests
    {
        [Test]
        public void LShape_MovesAllCells()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "######",
                    "#L...#",
                    "#LL..#",
                    "#....#",
                    "######")
                .Block('L', 0)
                .Build());
            var l = (Block)board.EntityAt(new Cell(1, 3));

            MoveResult result = new BlockMover(board).TryMove(l, Direction.Right);

            Assert.That(result, Is.EqualTo(MoveResult.Moved));
            Assert.That(board.EntityAt(new Cell(2, 3)), Is.SameAs(l));
            Assert.That(board.EntityAt(new Cell(2, 2)), Is.SameAs(l));
            Assert.That(board.EntityAt(new Cell(3, 2)), Is.SameAs(l));
            Assert.That(board.EntityAt(new Cell(1, 3)), Is.Null);
            Assert.That(board.EntityAt(new Cell(1, 2)), Is.Null);
        }

        [Test]
        public void UShape_IsBlocked_ByBlockInsideItsRecess()
        {
            // Moving up, U's bottom middle cell would enter B's cell.
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#...#",
                    "#UBU#",
                    "#UUU#",
                    "#####")
                .Block('U', 0)
                .Block('B', 1)
                .Build());
            var u = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(u, Direction.Up);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(u.Position, Is.EqualTo(new Cell(1, 1)));
        }
    }
}
