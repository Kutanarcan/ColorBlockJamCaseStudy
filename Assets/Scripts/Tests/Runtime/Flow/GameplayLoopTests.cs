using System.Collections.Generic;
using Game.Core;
using Game.Meta;
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
        private TestMeta meta;
        private ContinuePrice continuePrice;
        private List<string> log;
        private FakeStep winPanel;

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

            log = new List<string>();
            exitStep = new FakeStep("Exit", log);
            winPanel = new FakeStep("Win panel", log);
            exitSteps = new FakeExitSteps(exitStep);
            blocks = new FakeBlocksView();
            inputLock = new InputLock();
            exits = new Sequencer();
            flow = new Sequencer();

            meta = new TestMeta(startCoins: 100, reward: 50);
            var director = new GameplayDirector(exitSteps, inputLock, exits, flow, 0f, meta.Completion,
                new Panels(winPanel, new FakeStep("Fail panel", log)));
            session.Observer = director;
            var drag = new DragController(session, blocks, new FakePointerInput(), inputLock, new DragResolver(session),
                new DragSettings(0f, 0f, 0.3f), new FakeSfxPlayer(), new FakeBlockSelection());
            continuePrice = new ContinuePrice(900, 1000);
            loop = new GameplayLoop(session, director, drag, blocks, inputLock, continuePrice);
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
        public void Restart_PutsTheContinuePriceBackToItsBase()
        {
            var wallet = new TestMeta(startCoins: 5000).Wallet;
            continuePrice.TryPay(wallet);
            Assert.That(continuePrice.Current, Is.EqualTo(1900), "precondition: one continue paid");

            loop.Restart();

            Assert.That(continuePrice.Current, Is.EqualTo(900));
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

        [Test]
        public void Win_IsRecorded_TheMomentItHappens_BeforeThePopup()
        {
            session.TryMove(block, Direction.Left);

            Assert.That(flow.IsIdle, Is.False, "precondition: the win popup is still waiting for the exit");
            Assert.That(meta.Wallet.Coins, Is.EqualTo(150), "the reward is in");
            Assert.That(meta.Progression.LevelNumber, Is.EqualTo(2), "the next level is set");
        }

        [Test]
        public void Director_ShowsWin_AfterTheLastExitStep()
        {
            session.TryMove(block, Direction.Left);

            Assert.That(log, Is.EqualTo(new[] { "Exit started" }), "the panel waits for the exit");

            exitStep.Finish();

            Assert.That(log, Is.EqualTo(new[] { "Exit started", "Win panel started" }));
            Assert.That(inputLock.IsLocked, "the board stays locked under the panel");
        }

        /// <summary>The level-end panels as given steps.</summary>
        private sealed class Panels : ILevelPanels
        {
            private readonly IStep win;
            private readonly IStep fail;

            public Panels(IStep win, IStep fail)
            {
                this.win = win;
                this.fail = fail;
            }

            public IStep Win() => win;

            public IStep Fail() => fail;
        }
    }
}
