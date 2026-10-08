using Game.LevelTest;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.LevelTest
{
    public sealed class MemorySaveStoreTests
    {
        [Test]
        public void MemorySaveStore_GivesAFreshSection_BeforeTheFirstWrite()
        {
            Assert.That(new MemorySaveStore().Load<SettingsData>(Settings.SaveKey).music, Is.True);
        }

        [Test]
        public void MemorySaveStore_GivesACopy_NotTheObjectItWasGiven()
        {
            var store = new MemorySaveStore();
            var written = new ProgressionData { levelsCompleted = 3 };
            store.Save(Progression.SaveKey, written);

            written.levelsCompleted = 9;
            var read = store.Load<ProgressionData>(Progression.SaveKey);

            Assert.That(read, Is.Not.SameAs(written));
            Assert.That(read.levelsCompleted, Is.EqualTo(3));
        }
    }
}
