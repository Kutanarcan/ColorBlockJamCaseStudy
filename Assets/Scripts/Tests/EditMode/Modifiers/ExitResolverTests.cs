using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ExitResolverTests
    {
        private static readonly Cell TCell = new Cell(1, 1);
        private static readonly Cell ACell = new Cell(3, 1);

        // A sits above a matching door; one push down is one exit.
        private static LevelSession NewSession(params ModifierData[] tModifiers) =>
            new LevelSession(AsciiLevel.Parse(
                    "#####",
                    "#T.A#",
                    "###1#")
                .Block('T', 1, tModifiers)
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        [Test]
        public void DepletedModifier_IsRemoved_LeavingAPlainBlock()
        {
            LevelSession session = NewSession(new IceData { Count = 1 });
            Block t = BlockAt(session, TCell);

            session.TryMove(BlockAt(session, ACell), Direction.Down);

            Assert.That(t.ModifierCount, Is.EqualTo(0));
            Assert.That(session.TryMove(t, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void Removal_KeepsTheRemainingModifiersInOrder()
        {
            LevelSession session = NewSession(
                new IceData { Count = 1 },
                new ArrowData { Direction = Direction.Right },
                new IceData { Count = 5 });
            Block t = BlockAt(session, TCell);

            session.TryMove(BlockAt(session, ACell), Direction.Down);

            Assert.That(t.ModifierCount, Is.EqualTo(2));
            Assert.That(t.GetModifier(0), Is.InstanceOf<Arrow>());
            Assert.That(((Ice)t.GetModifier(1)).Durability.Remaining, Is.EqualTo(4));
        }
    }
}
