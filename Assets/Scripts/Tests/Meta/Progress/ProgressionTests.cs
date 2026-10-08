using System;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.Meta
{
    public sealed class ProgressionTests
    {
        private const int LevelCount = 5;

        [Test]
        public void Progression_WrapsToTheFirstLevel_AfterTheLast()
        {
            var progression = new Progression(new FakeSaveStore());

            for (int level = 0; level < LevelCount; level++)
            {
                Assert.That(progression.LevelIndex(LevelCount), Is.EqualTo(level));
                progression.CompleteLevel();
            }

            Assert.That(progression.LevelIndex(LevelCount), Is.Zero);
            Assert.That(progression.LevelNumber, Is.EqualTo(LevelCount + 1), "The number keeps counting.");
        }

        [Test]
        public void Progression_IsSaved_OnEveryCompletedLevel_AndSurvivesARestart()
        {
            var store = new FakeSaveStore();
            var progression = new Progression(store);

            progression.CompleteLevel();
            progression.CompleteLevel();

            Assert.That(store.WritesOf(Progression.SaveKey), Is.EqualTo(2));
            Assert.That(new Progression(store).LevelIndex(LevelCount), Is.EqualTo(2));
        }

        [Test]
        public void Progression_StaysInsideAShorterLevelList()
        {
            var store = new FakeSaveStore();
            store.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 7 });

            Assert.That(new Progression(store).LevelIndex(3), Is.EqualTo(1));
        }

        [Test]
        public void Progression_StartsAtTheFirstLevel_InAFreshSave_WithoutWriting()
        {
            var store = new FakeSaveStore();

            Assert.That(new Progression(store).LevelNumber, Is.EqualTo(1));
            Assert.That(store.Has(Progression.SaveKey), Is.False, "Reading writes nothing.");
        }

        [Test]
        public void Progression_RejectsAnEmptyLevelList()
        {
            var progression = new Progression(new FakeSaveStore());

            Assert.Throws<ArgumentOutOfRangeException>(() => progression.LevelIndex(0));
        }
    }
}
