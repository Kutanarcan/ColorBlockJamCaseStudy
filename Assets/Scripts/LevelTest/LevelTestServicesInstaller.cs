using Game.Infrastructure;
using Game.Meta;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.LevelTest
{
    /// <summary>
    /// The services a level test runs on (D130, D131): meta runs for real on a save kept in memory, the tested level is
    /// played every time, straight away (the first scene is Gameplay, not Home), and the wallet starts with the test's
    /// coins. Only the player's settings are copied in from the live save, once, while installing; the container
    /// never sees the live store, so nothing in a test can write it.
    /// It also puts the "LEVEL TEST" badge on screen for the whole run, so live code never makes or checks it (D135).
    /// A later cross-cutting service registers its null or in-memory stand-in here.
    /// </summary>
    public sealed class LevelTestServicesInstaller : IInstaller
    {
        private readonly string levelKey;
        private readonly ISaveStore liveSave;
        private readonly LevelTestConfig config;
        private readonly Transform badgeParent;

        public LevelTestServicesInstaller(string levelKey, ISaveStore liveSave, LevelTestConfig config,
            Transform badgeParent)
        {
            this.levelKey = levelKey;
            this.liveSave = liveSave;
            this.config = config;
            this.badgeParent = badgeParent;
        }

        public void Install(IContainerBuilder builder)
        {
            var testSave = new MemorySaveStore();
            testSave.Save(Settings.SaveKey, liveSave.Load<SettingsData>(Settings.SaveKey));

            builder.RegisterInstance<ISaveStore>(testSave);
            builder.RegisterInstance<ILevelChoice>(new FixedLevelChoice(levelKey));
            builder.RegisterInstance<IStartingCoins>(config);
            builder.RegisterInstance(new FirstScene(SceneKeys.Gameplay));
            builder.RegisterBuildCallback(_ => Object.Instantiate(config.Badge, badgeParent));
        }
    }
}
