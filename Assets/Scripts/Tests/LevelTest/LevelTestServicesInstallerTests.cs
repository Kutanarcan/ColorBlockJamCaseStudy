using Game.Infrastructure;
using Game.LevelTest;
using Game.Meta;
using NUnit.Framework;
using VContainer;

namespace Game.Tests.LevelTest
{
    public sealed class LevelTestServicesInstallerTests
    {
        private MemorySaveStore liveSave;

        [SetUp]
        public void SetUp()
        {
            liveSave = new MemorySaveStore();
            liveSave.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 2 });
            liveSave.Save(Settings.SaveKey, new SettingsData { sound = false });
        }

        [Test]
        public void LevelTestLaunch_KeepsTheLiveSaveUntouched()
        {
            using IObjectResolver container = Build("Level_6");
            var progression = container.Resolve<Progression>();
            var settings = container.Resolve<Settings>();

            Assert.That(settings.IsOn(Setting.Sound), Is.False, "The player's settings are copied in.");
            Assert.That(progression.LevelNumber, Is.EqualTo(1), "Progression starts fresh.");

            progression.CompleteLevel();
            settings.Set(Setting.Music, false);

            Assert.That(container.Resolve<ISaveStore>(), Is.Not.SameAs(liveSave));
            Assert.That(liveSave.Load<ProgressionData>(Progression.SaveKey).levelsCompleted, Is.EqualTo(2));
            Assert.That(liveSave.Load<SettingsData>(Settings.SaveKey).music, Is.True);
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

        /// <summary>The meta and level selection the game's root installer registers, on the level test's services.</summary>
        private IObjectResolver Build(string levelKey)
        {
            var builder = new ContainerBuilder();
            builder.Register<Progression>(Lifetime.Singleton);
            builder.Register<Settings>(Lifetime.Singleton);
            builder.Register<SelectedLevel>(Lifetime.Singleton);
            new LevelTestServicesInstaller(levelKey, liveSave).Install(builder);

            return builder.Build();
        }
    }
}
