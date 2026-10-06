using System.Collections.Generic;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public class SequencerTests
    {
        [Test]
        public void Sequencer_PlaysInOrder_GroupsInParallel_AndCancels()
        {
            var log = new List<string>();
            var a = new FakeStep("A", log);
            var b = new FakeStep("B", log);
            var c = new FakeStep("C", log);
            var d = new FakeStep("D", log);
            var e = new FakeStep("E", log);
            var sequencer = new Sequencer();

            sequencer.Run(new StepSequence(a, new StepGroup(b, c), d, e));
            Assert.That(log, Is.EqualTo(new[] { "A started" }));

            a.Finish();
            Assert.That(log, Is.EqualTo(new[] { "A started", "B started", "C started" }), "the group starts together");

            b.Finish();
            Assert.That(log.Count, Is.EqualTo(3), "the group waits for its last step");

            c.Finish();
            Assert.That(log[3], Is.EqualTo("D started"));

            sequencer.CancelAll();
            Assert.That(d.WasCancelled, "the running step stops");
            Assert.That(log, Has.No.Member("E started"), "nothing after it starts");
            Assert.That(sequencer.IsIdle);
        }

        [Test]
        public void WaitForIdle_HoldsTheFlow_UntilTheLastRunningStepFinishes()
        {
            var log = new List<string>();
            var firstExit = new FakeStep("Exit 1", log);
            var secondExit = new FakeStep("Exit 2", log);
            var popup = new FakeStep("Popup", log);
            var exits = new Sequencer();
            var flow = new Sequencer();

            exits.Run(firstExit);
            exits.Run(secondExit);
            flow.Run(new StepSequence(new WaitForIdleStep(exits), popup));

            firstExit.Finish();
            Assert.That(log, Has.No.Member("Popup started"), "one exit still runs");

            secondExit.Finish();
            Assert.That(log, Has.Member("Popup started"));
        }

        [Test]
        public void CancellingTheFlowFirst_KeepsAWaitingPopupFromShowing()
        {
            var log = new List<string>();
            var exit = new FakeStep("Exit", log);
            var popup = new FakeStep("Popup", log);
            var exits = new Sequencer();
            var flow = new Sequencer();

            exits.Run(exit);
            flow.Run(new StepSequence(new WaitForIdleStep(exits), popup));

            flow.CancelAll();
            exits.CancelAll();

            Assert.That(log, Has.No.Member("Popup started"));
            Assert.That(flow.IsIdle && exits.IsIdle);
        }
    }
}
