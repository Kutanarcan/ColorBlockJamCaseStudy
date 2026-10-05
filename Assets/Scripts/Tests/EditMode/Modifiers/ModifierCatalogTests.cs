using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ModifierCatalogTests
    {
        [Test]
        public void Default_KnowsTheV1Modifiers()
        {
            ModifierCatalog catalog = ModifierCatalog.Default();

            Assert.That(catalog.TryCreate("ice", out ModifierData ice), Is.True);
            Assert.That(ice, Is.InstanceOf<IceData>());
            Assert.That(catalog.TryCreate("arrow", out ModifierData arrow), Is.True);
            Assert.That(arrow, Is.InstanceOf<ArrowData>());
        }

        [Test]
        public void TryCreate_UnknownName_Fails()
        {
            Assert.That(ModifierCatalog.Default().TryCreate("laser", out ModifierData data), Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void TryCreate_ReturnsAFreshInstance_EachTime()
        {
            ModifierCatalog catalog = ModifierCatalog.Default();
            catalog.TryCreate("ice", out ModifierData first);
            catalog.TryCreate("ice", out ModifierData second);

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void Add_SameNameTwice_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => ModifierCatalog.Default().Add(() => new IceData()));
        }
    }
}
