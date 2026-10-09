using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class IceLookTests
    {
        private List<string> log;
        private Ice ice;
        private IceLook look;

        [SetUp]
        public void SetUp()
        {
            log = new List<string>();
            ice = new Ice(2);
            look = new IceLook(new View(log), ice, () => log.Add("thaw"));
        }

        [Test]
        public void IceLook_ShakesTheLowerCount_AndMeltsAtZero()
        {
            ice.OnExited(null, null, new Commands());
            look.Refresh();
            ice.OnExited(null, null, new Commands());
            look.Refresh();

            Assert.That(log, Is.EqualTo(new[] { "bump 1", "melt", "thaw" }));
        }

        [Test]
        public void IceLook_ShowsNothing_WhenTheCountDidNotChange()
        {
            look.Refresh();
            ice.OnExited(null, null, new Commands());
            look.Refresh();
            look.Refresh();

            Assert.That(log, Is.EqualTo(new[] { "bump 1" }), "one exit, one shake");
        }

        /// <summary>Writes "bump N" and "melt" to the shared log.</summary>
        private sealed class View : IIceView
        {
            private readonly List<string> log;

            public View(List<string> log) => this.log = log;

            public void Bump(int remaining) => log.Add("bump " + remaining);

            public void Melt() => log.Add("melt");
        }

        /// <summary>Ice asks to remove itself at zero; the look reads the count, so nothing happens here.</summary>
        private sealed class Commands : ILevelCommands
        {
            public void AddModifier(Entity entity, IModifier modifier) { }

            public void RemoveModifier(Entity entity, IModifier modifier) { }

            public void MoveEntity(Entity entity, Cell offset) { }

            public void Fail() { }

            public void AddTime(float seconds) { }
        }
    }
}
