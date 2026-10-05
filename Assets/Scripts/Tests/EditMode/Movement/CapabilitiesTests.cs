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
            Board board = NewBoard(new FakeModifierData(() => new FakeExitBlocker()));
            var a = (Block)board.EntityAt(ACell);
            var mover = new BlockMover(board);

            Assert.That(mover.TryMove(a, Direction.Down), Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
            Assert.That(mover.TryMove(a, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }
    }
}
