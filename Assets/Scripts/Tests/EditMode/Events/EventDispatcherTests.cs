using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class EventDispatcherTests
    {
        private static readonly Cell TCell = new Cell(1, 1);
        private static readonly Cell ACell = new Cell(4, 1);
        private static readonly Cell BCell = new Cell(5, 1);

        // T has free cells to its right. A and B each sit above a door of their color: two exits available.
        private static LevelSession NewSession(ModifierData[] t, ModifierData[] a = null) =>
            LevelSession.TryCreate(AsciiLevel.Parse(
                    "#######",
                    "#T..AB#",
                    "####12#")
                .Block('T', 2, t)
                .Block('A', 0, a ?? new ModifierData[0])
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .TimeLimit(10f)
                .Build()).Value;

        private static ModifierData[] Mods(params ModifierData[] modifiers) => modifiers;

        private static ModifierData Fake(IModifier modifier) => new FakeModifierData(() => modifier);

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        private static void Exit(LevelSession session, Cell cell) =>
            session.TryMove(BlockAt(session, cell), Direction.Down);

        [Test]
        public void Listener_CallingFail_FailsTheLevel()
        {
            LevelSession session = NewSession(Mods(Fake(new FakeFailOnExit())));

            Exit(session, ACell);

            Assert.That(session.State, Is.EqualTo(GameState.Failed));
        }

        [Test]
        public void ExitListener_TogglesACapability_WithAddAndRemove()
        {
            LevelSession session = NewSession(Mods(Fake(new FakeExitToggle())));
            Block t = BlockAt(session, TCell);
            Assert.That(Capabilities.CanExit(t), Is.True);

            Exit(session, ACell);
            Assert.That(Capabilities.CanExit(t), Is.False);

            Exit(session, BCell);
            Assert.That(Capabilities.CanExit(t), Is.True);
        }

        [Test]
        public void ExitingBlock_HearsItsOwnExit()
        {
            LevelSession session = NewSession(
                Mods(Fake(new FakeTimeBonus(100f))),
                Mods(Fake(new FakeTimeBonus(5f))));

            Exit(session, ACell);

            Assert.That(session.RemainingTime, Is.EqualTo(15f));
        }

        [Test]
        public void Commands_AreDeferred_UntilThePassEnds()
        {
            LevelSession session = NewSession(Mods(Fake(new FakeArmOnExit())));

            Exit(session, ACell);
            Assert.That(session.State, Is.EqualTo(GameState.Playing));

            Exit(session, BCell);
            Assert.That(session.State, Is.EqualTo(GameState.Failed));
        }

        [TestCase(1, 2)]  // free cell to the right: moves
        [TestCase(-1, 1)] // wall to the left: skipped
        public void MoveEntityCommand_MovesOnlyIntoFreeCells(int dx, int expectedX)
        {
            LevelSession session = NewSession(Mods(Fake(new FakeShiftOnExit(new Cell(dx, 0)))));
            Block t = BlockAt(session, TCell);

            Exit(session, ACell);

            Assert.That(t.Position, Is.EqualTo(new Cell(expectedX, 1)));
            Assert.That(session.Board.EntityAt(new Cell(expectedX, 1)), Is.SameAs(t));
        }

        [Test]
        public void TickListener_ReceivesDeltaTime()
        {
            var recorder = new FakeTickRecorder();
            LevelSession session = NewSession(Mods(Fake(recorder)));

            session.Tick(0.25f);
            session.Tick(0.25f);

            Assert.That(recorder.Total, Is.EqualTo(0.5f));
        }

        [Test]
        public void MoveListener_CanFailTheLevel_AfterCommittedMoves()
        {
            LevelSession session = NewSession(Mods(Fake(new FakeMoveLimit(2))));
            Block t = BlockAt(session, TCell);

            session.TryMove(t, Direction.Right);
            session.CommitMove();
            Assert.That(session.State, Is.EqualTo(GameState.Playing));

            session.TryMove(t, Direction.Right);
            session.CommitMove();
            Assert.That(session.State, Is.EqualTo(GameState.Failed));
        }

        [Test]
        public void DepletedDurability_IsRemoved_LeavingAPlainBlock()
        {
            LevelSession session = NewSession(Mods(new IceData { Count = 1 }));
            Block t = BlockAt(session, TCell);

            Exit(session, ACell);

            Assert.That(t.ModifierCount, Is.EqualTo(0));
            Assert.That(session.TryMove(t, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void Removal_KeepsTheRemainingModifiersInOrder()
        {
            LevelSession session = NewSession(Mods(
                new IceData { Count = 1 },
                new ArrowData { Direction = Direction.Right },
                new IceData { Count = 5 }));
            Block t = BlockAt(session, TCell);

            Exit(session, ACell);

            Assert.That(t.ModifierCount, Is.EqualTo(2));
            Assert.That(t.GetModifier(0), Is.InstanceOf<Arrow>());
            Assert.That(((Ice)t.GetModifier(1)).Durability.Remaining, Is.EqualTo(4));
        }
    }
}
