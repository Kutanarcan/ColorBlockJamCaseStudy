using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelSessionTests
    {
        private static readonly Cell ACell = new Cell(1, 1);
        private static readonly Cell BCell = new Cell(4, 1);

        // A and B each sit right above a door of their color: one push down exits.
        private static LevelSession NewSession() =>
            new LevelSession(AsciiLevel.Parse(
                    "######",
                    "#A..B#",
                    "#11#2#")
                .Block('A', 0)
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Door('2', 1, Direction.Down)
                .TimeLimit(10f)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        [Test]
        public void ExitWithBlocksLeft_KeepsPlaying()
        {
            LevelSession session = NewSession();

            session.TryMove(BlockAt(session, ACell), Direction.Down);

            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(BlockAt(session, BCell).IsExited, Is.False);
        }

        [Test]
        public void LastBlockExit_WinsTheLevel()
        {
            LevelSession session = NewSession();

            session.TryMove(BlockAt(session, ACell), Direction.Down);
            session.TryMove(BlockAt(session, BCell), Direction.Down);

            Assert.That(session.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void TimeRunsOut_FailsTheLevel()
        {
            LevelSession session = NewSession();

            session.Tick(4f);
            Assert.That(session.State, Is.EqualTo(GameState.Playing));

            session.Tick(6f);
            Assert.That(session.State, Is.EqualTo(GameState.Failed));
            Assert.That(session.RemainingTime, Is.EqualTo(0f));
        }

        [Test]
        public void Moves_AreIgnored_AfterFail()
        {
            LevelSession session = NewSession();
            Block a = BlockAt(session, ACell);
            session.Tick(10f);

            MoveResult result = session.TryMove(a, Direction.Down);

            Assert.That(result, Is.EqualTo(MoveResult.Blocked));
            Assert.That(a.IsExited, Is.False);
        }

        [Test]
        public void Tick_DoesNothing_AfterWin()
        {
            LevelSession session = NewSession();
            session.Tick(3f);
            session.TryMove(BlockAt(session, ACell), Direction.Down);
            session.TryMove(BlockAt(session, BCell), Direction.Down);

            session.Tick(100f);

            Assert.That(session.State, Is.EqualTo(GameState.Won));
            Assert.That(session.RemainingTime, Is.EqualTo(7f));
        }

        [Test]
        public void AddTime_AfterFail_ResumesPlaying()
        {
            LevelSession session = NewSession();
            session.Tick(10f);

            session.AddTime(5f);

            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(session.RemainingTime, Is.EqualTo(5f));
            Assert.That(session.TryMove(BlockAt(session, ACell), Direction.Down), Is.EqualTo(MoveResult.Exited));
        }

        [Test]
        public void CommitMove_CountsOnly_WhenABlockMoved()
        {
            LevelSession session = NewSession();

            session.CommitMove();
            Assert.That(session.MoveCount, Is.EqualTo(0));

            session.TryMove(BlockAt(session, ACell), Direction.Right);
            session.CommitMove();
            Assert.That(session.MoveCount, Is.EqualTo(1));

            session.CommitMove();
            Assert.That(session.MoveCount, Is.EqualTo(1));
        }

        [Test]
        public void Exit_EndsTheMove_SoACommitAfterItDoesNotCountTwice()
        {
            LevelSession session = NewSession();

            session.TryMove(BlockAt(session, ACell), Direction.Down);
            session.CommitMove();

            Assert.That(session.MoveCount, Is.EqualTo(1));
        }

        [Test]
        public void Restart_RestoresInitialState()
        {
            LevelSession session = NewSession();
            Board oldBoard = session.Board;
            session.TryMove(BlockAt(session, ACell), Direction.Down);
            session.Tick(10f);

            session.Restart();

            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(session.RemainingTime, Is.EqualTo(10f));
            Assert.That(session.Board, Is.Not.SameAs(oldBoard));
            Assert.That(session.MoveCount, Is.EqualTo(0));
            Assert.That(BlockAt(session, ACell).IsExited, Is.False);
            Assert.That(BlockAt(session, BCell).IsExited, Is.False);
        }
    }
}
