using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ExitRuleTests
    {
        [Test]
        public void Bar_PushedIntoMatchingDoor_Exits()
        {
            Board board = BarOverDoor("#11##", 0, Direction.Down);
            var a = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
            Assert.That(a.IsExited, Is.True);
            Assert.That(board.EntityAt(new Cell(1, 1)), Is.Null);
            Assert.That(board.EntityAt(new Cell(2, 1)), Is.Null);
        }

        [Test]
        public void ExitedBlock_CannotMoveAgain()
        {
            Board board = BarOverDoor("#11##", 0, Direction.Down);
            var a = (Block)board.EntityAt(new Cell(1, 1));
            var mover = new BlockMover(board);
            mover.TryMove(a, Direction.Down);

            MoveResult result = mover.TryMove(a, Direction.Up);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(board.EntityAt(new Cell(1, 1)), Is.Null);
        }

        [TestCase("#11##", 1, Direction.Down)] // wrong color
        [TestCase("#11##", 0, Direction.Up)]   // wrong direction
        [TestCase("#1###", 0, Direction.Down)] // door narrower than the block
        public void Bar_DoesNotExit_WhenDoorDoesNotMatch(string doorRow, int doorColor, Direction doorDirection)
        {
            Board board = BarOverDoor(doorRow, doorColor, doorDirection);
            var a = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
            Assert.That(board.EntityAt(new Cell(1, 1)), Is.SameAs(a));
            Assert.That(board.EntityAt(new Cell(2, 1)), Is.SameAs(a));
        }

        [Test]
        public void UShape_OpenTowardDoor_Exits()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#UUU#",
                    "#U.U#",
                    "#111#")
                .Block('U', 0)
                .Door('1', 0, Direction.Down)
                .Build());
            var u = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(u, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
        }

        [Test]
        public void UShape_WithBlockInRecess_DoesNotExit()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#UUU#",
                    "#UBU#",
                    "#111#")
                .Block('U', 0)
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Build());
            var u = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(u, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(u.IsExited, Is.False);
            Assert.That(board.EntityAt(new Cell(2, 1)), Is.InstanceOf<Block>().And.Not.SameAs(u));
        }

        [Test]
        public void CShape_WithGapInColumn_ExitsThroughItsFrontCell()
        {
            // Column 1 holds C at y = 3 and y = 1 with a gap between; the scan from y = 3 meets C itself.
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#CC.#",
                    "#.C.#",
                    "#CC.#",
                    "#11##")
                .Block('C', 0)
                .Door('1', 0, Direction.Down)
                .Build());
            var c = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(c, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
        }

        [Test]
        public void Bar_ExitsAcross_TwoDoorEntities_OfSameColorAndDirection()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#AA.#",
                    "#12##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Door('2', 0, Direction.Down)
                .Build());
            var a = (Block)board.EntityAt(new Cell(1, 1));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
        }

        [Test]
        public void InnerDoor_AcceptsBlock_MovingInItsDirection()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#######",
                    "#.....#",
                    "#..1..#",
                    "#..A..#",
                    "#######")
                .Block('A', 0)
                .Door('1', 0, Direction.Up)
                .Build());
            var a = (Block)board.EntityAt(new Cell(3, 1));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Up);

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
        }

        [Test]
        public void InnerDoor_FromTheOtherSide_ActsAsWall()
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#######",
                    "#..A..#",
                    "#..1..#",
                    "#.....#",
                    "#######")
                .Block('A', 0)
                .Door('1', 0, Direction.Up)
                .Build());
            var a = (Block)board.EntityAt(new Cell(3, 3));

            MoveResult result = new BlockMover(board).TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
        }

        private static Board BarOverDoor(string doorRow, int doorColor, Direction doorDirection) =>
            BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#AA.#",
                    doorRow)
                .Block('A', 0)
                .Door('1', doorColor, doorDirection)
                .Build());
    }
}
