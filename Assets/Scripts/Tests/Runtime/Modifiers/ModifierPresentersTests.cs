using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public class ModifierPresentersTests
    {
        /// <summary>A modifier no presenter knows: it has no look.</summary>
        private sealed class UnknownModifier : IModifier
        {
        }

        // Views are not needed to choose a presenter, so the presenters get none.
        private static ModifierPresenters NewPresenters() =>
            new ModifierPresenters(new IModifierPresenter[] { new IcePresenter(null), new ArrowPresenter(null) });

        [Test]
        public void EachModifier_IsDrawnByItsOwnPresenter()
        {
            ModifierPresenters presenters = NewPresenters();

            Assert.That(presenters.Find(new Ice(2)), Is.InstanceOf<IcePresenter>());
            Assert.That(presenters.Find(new Arrow(Axis.Vertical)), Is.InstanceOf<ArrowPresenter>());
        }

        [Test]
        public void ModifierWithoutAPresenter_HasNoLook()
        {
            Assert.That(NewPresenters().Find(new UnknownModifier()), Is.Null);
        }
    }
}
