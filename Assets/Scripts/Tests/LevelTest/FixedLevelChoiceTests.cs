using System;
using Game.LevelTest;
using NUnit.Framework;

namespace Game.Tests.LevelTest
{
    public sealed class FixedLevelChoiceTests
    {
        [Test]
        public void FixedLevelChoice_AlwaysChoosesItsLevel()
        {
            var choice = new FixedLevelChoice("Level_6");

            Assert.That(choice.Choose(), Is.EqualTo("Level_6"));
            Assert.That(choice.Choose(), Is.EqualTo("Level_6"));
        }

        [Test]
        public void FixedLevelChoice_RefusesAnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => new FixedLevelChoice(""));
        }
    }
}
