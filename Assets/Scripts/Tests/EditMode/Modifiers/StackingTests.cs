using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class StackingTests
    {
        private static readonly Cell RCell = new Cell(2, 1);
        private static readonly Cell ACell = new Cell(5, 1);
        private static readonly Cell BCell = new Cell(6, 1);

        // R has free cells on both sides. A and B each sit above a door of their color: two exits available.
        private static LevelSession NewSession(params ModifierData[] rModifiers) =>
            new LevelSession(AsciiLevel.Parse(
                    "########",
                    "#.R..AB#",
                    "#####12#")
                .Block('R', 2, rModifiers)
                .Block('A', 0)
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        private static void ExitBoth(LevelSession session)
        {
            session.TryMove(BlockAt(session, ACell), Direction.Down);
            session.TryMove(BlockAt(session, BCell), Direction.Down);
        }

        [Test]
        public void IceAndArrow_ArrowAppliesAfterThaw()
        {
            LevelSession session = NewSession(
                new IceData { Count = 2 },
                new ArrowData { Direction = Direction.Right });
            Block r = BlockAt(session, RCell);
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            ExitBoth(session);

            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Blocked));
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void TwoIces_ProgressInParallel_EachRemovedWhenDepleted()
        {
            LevelSession session = NewSession(new IceData { Count = 1 }, new IceData { Count = 2 });
            Block r = BlockAt(session, RCell);

            session.TryMove(BlockAt(session, ACell), Direction.Down);

            Assert.That(r.ModifierCount, Is.EqualTo(1));
            Assert.That(((Ice)r.GetModifier(0)).Durability.Remaining, Is.EqualTo(1));
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, BCell), Direction.Down);

            Assert.That(r.ModifierCount, Is.EqualTo(0));
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }
    }
}
