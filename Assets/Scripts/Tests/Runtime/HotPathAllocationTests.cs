using Game.Core;
using Game.Meta;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests.Runtime
{
    /// <summary>
    /// P8.3 evidence: the per-frame paths allocate nothing (§8 hot paths). Each path runs once before it is measured,
    /// so first-call costs (JIT, static setup) are not counted. The views are left out; the Profiler capture covers them.
    /// </summary>
    public class HotPathAllocationTests
    {
        private LevelSession session;
        private Block block;
        private DragResolver resolver;
        private GameplayLoop loop;
        private Sequencer exits;
        private Sequencer flow;

        [SetUp]
        public void SetUp()
        {
            LevelData level = LevelMap.Parse(Direction.Left,
                "#######",
                "#.....#",
                "#######");
            level.TimeLimit = 600f;
            level.Blocks = new[] { new BlockData { ColorId = 0, Cells = new[] { new Cell(1, 1) } } };
            session = LevelSession.TryCreate(level).Value;
            block = (Block)session.Board.EntityAt(new Cell(1, 1));

            var inputLock = new InputLock();
            exits = new Sequencer();
            flow = new Sequencer();
            // No door: the level never ends here, so no panel is ever asked for.
            var director = new GameplayDirector(new FakeExitSteps(null), inputLock, exits, flow, 0f,
                new TestMeta().Completion, null);
            session.Observer = director;

            var blocks = new FakeBlocksView();
            resolver = new DragResolver(session);
            var drag = new DragController(session, blocks, new FakePointerInput(), inputLock, resolver,
                new DragSettings(0f, 0f, 0.3f), new FakeSfxPlayer(), new FakeBlockSelection());
            loop = new GameplayLoop(session, director, drag, blocks, inputLock, new ContinuePrice(900, 1000));
        }

        [TearDown]
        public void TearDown()
        {
            exits.Dispose();
            flow.Dispose();
        }

        [Test]
        public void Frame_SessionTickAndIdleDrag_AllocateNothing()
        {
            loop.Tick(0.016f);

            Assert.That(() => loop.Tick(0.016f), Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Drag_MoveTowardAndClamp_AllocateNothing()
        {
            resolver.MoveToward(block, new Cell(4, 1));
            resolver.ClampToReachable(block, new Vector2(4.3f, 1.2f));
            resolver.MoveToward(block, new Cell(1, 1));

            // Statement bodies: the constraint needs a TestDelegate to run, not a value to compare.
            Assert.That(() => { resolver.MoveToward(block, new Cell(4, 1)); }, Is.Not.AllocatingGCMemory(),
                "moves and the observer");
            Assert.That(() => { resolver.ClampToReachable(block, new Vector2(4.3f, 1.2f)); },
                Is.Not.AllocatingGCMemory(), "the lean's previews");
        }
    }
}
