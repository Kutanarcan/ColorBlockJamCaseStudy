using System;
using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public sealed class SelectedLevelTests
    {
        [Test]
        public void SelectedLevel_Throws_BeforeAnythingIsSelected()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new SelectedLevel(new CountingChoice()).Key);
        }

        [Test]
        public void SelectedLevel_AsksItsChoice_OnEverySelect()
        {
            var level = new SelectedLevel(new CountingChoice());

            level.Select();
            Assert.That(level.Key, Is.EqualTo("Level_1"));
            level.Select();
            Assert.That(level.Key, Is.EqualTo("Level_2"));
        }

        private sealed class CountingChoice : ILevelChoice
        {
            private int calls;

            public string Choose() => "Level_" + ++calls;
        }
    }
}
