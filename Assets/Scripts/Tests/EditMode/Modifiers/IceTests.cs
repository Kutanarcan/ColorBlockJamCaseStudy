using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class IceTests
    {
        private static readonly Cell TCell = new Cell(1, 1);
        private static readonly Cell ACell = new Cell(3, 1);
        private static readonly Cell BCell = new Cell(4, 1);

        // T is frozen for 2 exits. A and B each sit above a door of their color.
        private static LevelSession NewSession() =>
            LevelSession.TryCreate(AsciiLevel.Parse(
                    "#######",
                    "#T.AB.#",
                    "###12##")
                .Block('T', 5, new IceData { Count = 2 })
                .Block('A', 0)
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .Build()).Value;

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        [Test]
        public void Ice_BlocksMove_UntilEnoughExitsHappened()
        {
            LevelSession session = NewSession();
            Block t = BlockAt(session, TCell);

            Assert.That(session.TryMove(t, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, ACell), Direction.Down);
            Assert.That(session.TryMove(t, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, BCell), Direction.Down);
            Assert.That(session.TryMove(t, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void Restart_ResetsIceCounter()
        {
            LevelSession session = NewSession();
            session.TryMove(BlockAt(session, ACell), Direction.Down);

            session.Restart();

            var ice = (Ice)BlockAt(session, TCell).GetModifier(0);
            Assert.That(ice.Durability.Remaining, Is.EqualTo(2));
        }
    }
}
