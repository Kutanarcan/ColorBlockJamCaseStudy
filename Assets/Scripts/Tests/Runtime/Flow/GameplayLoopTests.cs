using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public class GameplayLoopTests
    {
        private const float TimeLimit = 60f;

        private LevelSession session;
        private Block block;
        private FakeStep exitStep;
        private FakeExitSteps exitSteps;
        private FakeBlocksView blocks;
        private InputLock inputLock;
        private Sequencer exits;
        private Sequencer flow;
        private GameplayLoop loop;

        /// <summary>One block beside its door: a single move exits it and wins the level.</summary>
        [SetUp]
        public void SetUp()
        {
            LevelData level = LevelMap.Parse(Direction.Left,
                "####",
                "0..#",
                "####");
            level.TimeLimit = TimeLimit;
            level.Blocks = new[] { new BlockData { ColorId = 0, Cells = new[] { new Cell(1, 1) } } };
            session = LevelSession.TryCreate(level).Value;
            block = (Block)session.Board.EntityAt(new Cell(1, 1));

            exitStep = new FakeStep("Exit", new List<string>());
            exitSteps = new FakeExitSteps(exitStep);
            blocks = new FakeBlocksView();
            inputLock = new InputLock();
            exits = new Sequencer();
            flow = new Sequencer();

            var director = new GameplayDirector(exitSteps, inputLock, exits, flow, 0f);
            session.Observer = director;
            var drag = new DragController(session, blocks, new FakePointerInput(), inputLock, new DragResolver(session),
                new DragSettings(0f, 0f), new FakeSfxPlayer());
            loop = new GameplayLoop(session, director, drag, blocks, inputLock);
        }

        [TearDown]
        public void TearDown()
        {
            exits.Dispose();
            flow.Dispose();
        }

        [Test]
        public void Restart_LeavesNoViewOrStepFromTheLastAttempt()
        {
            session.TryMove(block, Direction.Left);
            Assert.That(session.State, Is.EqualTo(GameState.Won), "precondition: won, exit still playing");
            Assert.That(exits.IsIdle || flow.IsIdle, Is.False);

            loop.Restart();

            Assert.That(exitStep.WasCancelled, "the exit stops where it is");
            Assert.That(exits.IsIdle && flow.IsIdle, "no step and no waiting popup is left");
            Assert.That(exitSteps.ClearCount, Is.EqualTo(1), "flying particles are cleared");
            Assert.That(blocks.Calls, Is.EqualTo(new[] { "clear", "build" }));
            Assert.That(blocks.LastBuilt, Is.SameAs(session.Board), "views come from the restarted board");
            Assert.That(session.Board.EntityAt(new Cell(1, 1)), Is.InstanceOf<Block>(), "the block is back");
            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(inputLock.IsLocked, Is.False);
        }

        [Test]
        public void Pause_StopsTheTimer_AndLocksInput_UntilResumed()
        {
            loop.Pause();
            loop.Tick(5f);

            Assert.That(session.RemainingTime, Is.EqualTo(TimeLimit));
            Assert.That(inputLock.IsLocked);

            loop.Resume();
            loop.Tick(5f);

            Assert.That(session.RemainingTime, Is.EqualTo(TimeLimit - 5f));
            Assert.That(inputLock.IsLocked, Is.False);
        }

        [Test]
        public void Resume_AfterAWin_KeepsTheBoardLocked()
        {
            session.TryMove(block, Direction.Left);

            loop.Pause();
            loop.Resume();

            Assert.That(inputLock.IsLocked, "the win still holds the lock");
        }
    }
}
