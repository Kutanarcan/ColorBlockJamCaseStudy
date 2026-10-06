using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class SessionObserverTests
    {
        private static LevelSession Observed(LevelData level, out FakeSessionObserver observer)
        {
            LevelSession session = LevelSession.TryCreate(level).Value;
            observer = new FakeSessionObserver();
            session.Observer = observer;

            return session;
        }

        private static Block BlockAt(LevelSession session, int x, int y) =>
            (Block)session.Board.EntityAt(new Cell(x, y));

        [Test]
        public void Session_TellsTheObserver_MovesExitsRemovalsAndState()
        {
            // B is iced until one block exits; A walks over its door and exits, then B can follow.
            LevelSession session = Observed(AsciiLevel.Parse(
                    "######",
                    "#A..B#",
                    "#11#2#")
                .Block('A', 0)
                .Block('B', 1, new IceData { Count = 1 })
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .TimeLimit(10f)
                .Build(), out FakeSessionObserver observer);
            Block a = BlockAt(session, 1, 1);
            Block b = BlockAt(session, 4, 1);

            session.TryMove(a, Direction.Right);
            session.TryMove(a, Direction.Down);
            session.TryMove(b, Direction.Down);

            Assert.That(observer.Calls, Is.EqualTo(new[]
            {
                $"Moved {a.Id} (1, 0)",
                $"Exited {a.Id} Down",
                $"Removed {b.Id} Ice",
                $"Exited {b.Id} Down",
                "State Won"
            }));
        }

        [Test]
        public void BlockedStep_TellsTheObserverNothing()
        {
            LevelSession session = Observed(AsciiLevel.Parse(
                    "###",
                    "#A#",
                    "###")
                .Block('A', 0)
                .TimeLimit(10f)
                .Build(), out FakeSessionObserver observer);

            session.TryMove(BlockAt(session, 1, 1), Direction.Up);

            Assert.That(observer.Calls, Is.Empty);
        }

        [Test]
        public void Commands_TellTheObserver_EntityMovesAndAddedModifiers_InFlushOrder()
        {
            // On A's exit, T's listeners queue a shift right, then add a modifier.
            LevelSession session = Observed(AsciiLevel.Parse(
                    "######",
                    "#A.T.#",
                    "#1####")
                .Block('A', 0)
                .Block('T', 1,
                    new FakeModifierData(() => new FakeShiftOnExit(new Cell(1, 0))),
                    new FakeModifierData(() => new FakeArmOnExit()))
                .Door('1', 0, Direction.Down)
                .TimeLimit(10f)
                .Build(), out FakeSessionObserver observer);
            Block a = BlockAt(session, 1, 1);
            Block t = BlockAt(session, 3, 1);

            session.TryMove(a, Direction.Down);

            Assert.That(observer.Calls, Is.EqualTo(new[]
            {
                $"Exited {a.Id} Down",
                $"Moved {t.Id} (1, 0)",
                $"Added {t.Id} FakeFailOnExit"
            }));
        }

        [Test]
        public void Session_TellsTheObserver_FailContinueAndRestart()
        {
            LevelSession session = Observed(AsciiLevel.Parse(
                    "###",
                    "#A#",
                    "###")
                .Block('A', 0)
                .TimeLimit(10f)
                .Build(), out FakeSessionObserver observer);

            session.Tick(10f);
            session.AddTime(5f);
            session.Restart();

            Assert.That(observer.Calls, Is.EqualTo(new[] { "State Failed", "State Playing", "State Playing" }));
        }
    }
}
