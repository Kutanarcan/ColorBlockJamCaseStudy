using System.IO;
using System.Text.RegularExpressions;
using Game.Infrastructure;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.Infrastructure
{
    public sealed class JsonSaveStoreTests
    {
        private string folder;

        [SetUp]
        public void SetUp() =>
            folder = Path.Combine(Path.GetTempPath(), "JsonSaveStoreTests_" + TestContext.CurrentContext.Test.ID);

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        [Test]
        public void JsonSaveStore_GivesAFreshSection_BeforeTheFirstWrite()
        {
            var settings = new JsonSaveStore(folder).Load<SettingsData>(Settings.SaveKey);

            Assert.That(settings.sound, Is.True);
            Assert.That(Directory.Exists(folder), Is.False, "Reading creates nothing.");
        }

        [Test]
        public void JsonSaveStore_ReadsBackEachSection_InANewRun()
        {
            var store = new JsonSaveStore(folder);
            store.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 4 });
            store.Save(Settings.SaveKey, new SettingsData { sound = false });

            var next = new JsonSaveStore(folder);

            Assert.That(next.Load<ProgressionData>(Progression.SaveKey).levelsCompleted, Is.EqualTo(4));
            Assert.That(next.Load<SettingsData>(Settings.SaveKey).sound, Is.False);
            Assert.That(Directory.GetFiles(folder, "*.tmp"), Is.Empty, "No temporary file is left.");
        }

        [Test]
        public void JsonSaveStore_StartsOnlyTheUnreadableSectionFresh_WithAWarning()
        {
            var store = new JsonSaveStore(folder);
            store.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 4 });
            store.Save(Settings.SaveKey, new SettingsData { sound = false });
            File.WriteAllText(Path.Combine(folder, Settings.SaveKey + ".json"), "{ not json");
            LogAssert.Expect(LogType.Warning, new Regex("could not be read"));

            Assert.That(store.Load<SettingsData>(Settings.SaveKey).sound, Is.True);
            Assert.That(store.Load<ProgressionData>(Progression.SaveKey).levelsCompleted, Is.EqualTo(4));
        }
    }
}
