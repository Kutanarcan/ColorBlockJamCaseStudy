using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class CapabilitiesTests
    {
        private static readonly Cell ACell = new Cell(1, 1);

        // A sits above a matching door with a free cell to its right.
        private static Board NewBoard(params ModifierData[] modifiers) =>
            BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    "#A..#",
                    "#1###")
                .Block('A', 0, modifiers)
                .Door('1', 0, Direction.Down)
                .Build());

        [Test]
        public void PlainBlock_CanMoveAndExit()
        {
            Board board = NewBoard();
            var a = (Block)board.EntityAt(ACell);

            Assert.That(Capabilities.CanMove(a, Direction.Right), Is.True);
            Assert.That(Capabilities.CanExit(a), Is.True);
        }

        [Test]
        public void ExitOnlySuspender_AllowsMove_ButBlocksExit()
        {
            Board board = NewBoard(new FakeExitBlockerData());
            var a = (Block)board.EntityAt(ACell);
            var mover = new BlockMover(board);

            Assert.That(mover.TryMove(a, Direction.Down), Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
            Assert.That(mover.TryMove(a, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        // Door modifiers do not come from level data in V1; they are added directly,
        // the same way a listener would through the AddModifier command.
        [Test]
        public void LockedDoor_RejectsExit_UntilUnlocked()
        {
            Board board = NewBoard();
            var a = (Block)board.EntityAt(ACell);
            var door = (Door)board.EntityAt(new Cell(1, 0));
            var doorLock = new FakeDoorLock();
            var mover = new BlockMover(board);

            door.AddModifier(doorLock);
            Assert.That(mover.TryMove(a, Direction.Down), Is.EqualTo(MoveResult.Blocked));

            door.RemoveModifier(doorLock);
            Assert.That(mover.TryMove(a, Direction.Down), Is.EqualTo(MoveResult.Exited));
        }

        [TestCase("#AA.#", MoveResult.Blocked)] // bar needs both cells, one is closed
        [TestCase("#A..#", MoveResult.Exited)]  // single cell uses only the open one
        public void AcceptRule_CanCloseOneCellOfADoor(string blockRow, MoveResult expected)
        {
            Board board = BoardBuilder.Build(AsciiLevel.Parse(
                    "#####",
                    blockRow,
                    "#11##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build());
            var a = (Block)board.EntityAt(ACell);
            board.EntityAt(new Cell(1, 0)).AddModifier(new FakeClosedDoorCell(new Cell(2, 0)));

            Assert.That(new BlockMover(board).TryMove(a, Direction.Down), Is.EqualTo(expected));
        }
    }
}
