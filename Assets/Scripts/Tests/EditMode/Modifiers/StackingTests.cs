using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class StackingTests
    {
        private static readonly Cell RCell = new Cell(1, 1);
        private static readonly Cell ACell = new Cell(4, 1);
        private static readonly Cell SCell = new Cell(6, 1);

        // R carries a rope (color 3) and Ice(2). A is plain, S carries matching scissors;
        // both sit above a door of their color.
        private static LevelSession NewSession() =>
            new LevelSession(AsciiLevel.Parse(
                    "########",
                    "#R..A.S#",
                    "####1#2#")
                .Block('R', 2, new RopeData { ColorId = 3 }, new IceData { Count = 2 })
                .Block('A', 0)
                .Block('S', 1, new ScissorsData { ColorId = 3 })
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        [Test]
        public void RopeAndIce_BothMustResolve_BeforeMove()
        {
            LevelSession session = NewSession();
            Block r = BlockAt(session, RCell);

            session.TryMove(BlockAt(session, ACell), Direction.Down);
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, SCell), Direction.Down);
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void Ice_CountsDown_WhileRopeIsStillOn()
        {
            LevelSession session = NewSession();
            Block r = BlockAt(session, RCell);

            session.TryMove(BlockAt(session, ACell), Direction.Down);

            var rope = (Rope)r.GetModifier(0);
            var ice = (Ice)r.GetModifier(1);
            Assert.That(rope.Durability.Remaining, Is.EqualTo(1));
            Assert.That(ice.Durability.Remaining, Is.EqualTo(1));
        }
    }
}
