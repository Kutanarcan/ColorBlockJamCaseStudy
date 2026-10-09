using Game.Infrastructure;
using Game.LevelTest;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace Game.Tests.LevelTest
{
    public sealed class LevelTestServicesInstallerTests
    {
        private MemorySaveStore liveSave;
        private LevelTestConfig testConfig;
        private GameObject badge;
        private GameObject badgeParent;

        [SetUp]
        public void SetUp()
        {
            liveSave = new MemorySaveStore();
            liveSave.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 2 });
            liveSave.Save(Settings.SaveKey, new SettingsData { sound = false });
            liveSave.Save(Wallet.SaveKey, new WalletData { granted = true, coins = 30 });
            badge = new GameObject("Badge");
            badgeParent = new GameObject("LevelTestScope");
            testConfig = TestConfigs.WithBadge(badge);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testConfig);
            Object.DestroyImmediate(badge);
            Object.DestroyImmediate(badgeParent);
        }

        [Test]
        public void LevelTestLaunch_KeepsTheLiveSaveUntouched()
        {
            using IObjectResolver container = Build("Level_6");
            var progression = container.Resolve<Progression>();
            var settings = container.Resolve<Settings>();
            var wallet = container.Resolve<Wallet>();

            Assert.That(settings.IsOn(Setting.Sound), Is.False, "The player's settings are copied in.");
            Assert.That(progression.LevelNumber, Is.EqualTo(1), "Progression starts fresh.");
            Assert.That(wallet.Coins, Is.EqualTo(testConfig.Amount), "The wallet starts with the test's coins.");

            progression.CompleteLevel();
            settings.Set(Setting.Music, false);
            wallet.TrySpend(900);

            Assert.That(container.Resolve<ISaveStore>(), Is.Not.SameAs(liveSave));
            Assert.That(liveSave.Load<ProgressionData>(Progression.SaveKey).levelsCompleted, Is.EqualTo(2));
            Assert.That(liveSave.Load<SettingsData>(Settings.SaveKey).music, Is.True);
            Assert.That(liveSave.Load<WalletData>(Wallet.SaveKey).coins, Is.EqualTo(30));
        }

        [Test]
        public void LevelTestLaunch_PlaysTheTestedLevel_EveryTime()
        {
            using IObjectResolver container = Build("Level_6");
            var level = container.Resolve<SelectedLevel>();
            container.Resolve<Progression>().CompleteLevel();

            level.Select();
            Assert.That(level.Key, Is.EqualTo("Level_6"));
            level.Select();
            Assert.That(level.Key, Is.EqualTo("Level_6"), "A win replays the tested level.");
        }

        [Test]
        public void LevelTestLaunch_PutsTheBadgeOnScreen_ForTheWholeRun()
        {
            using IObjectResolver container = Build("Level_6");

            Assert.That(badgeParent.transform.childCount, Is.EqualTo(1), "One badge, under the level test's scope.");
            Assert.That(badgeParent.transform.GetChild(0).name, Does.StartWith(badge.name));
        }

        /// <summary>The meta and level selection the game's root installer registers, on the level test's services.</summary>
        private IObjectResolver Build(string levelKey)
        {
            var builder = new ContainerBuilder();
            builder.Register<Progression>(Lifetime.Singleton);
            builder.Register<Settings>(Lifetime.Singleton);
            builder.Register<Wallet>(Lifetime.Singleton);
            builder.Register<SelectedLevel>(Lifetime.Singleton);
            new LevelTestServicesInstaller(levelKey, liveSave, testConfig, badgeParent.transform).Install(builder);

            return builder.Build();
        }
    }
}
