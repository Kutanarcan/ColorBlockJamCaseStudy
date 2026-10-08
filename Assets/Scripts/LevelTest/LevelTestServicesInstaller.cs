using Game.Infrastructure;
using Game.Meta;
using VContainer;
using VContainer.Unity;

namespace Game.LevelTest
{
    /// <summary>
    /// The services a level test runs on (D130, D131): meta runs for real on a save kept in memory, the tested level is
    /// played every time and the wallet starts with the test's coins. Only the player's settings are copied in from the
    /// live save, once, while installing; the container never sees the live store, so nothing in a test can write it.
    /// A later cross-cutting service registers its null or in-memory stand-in here.
    /// </summary>
    public sealed class LevelTestServicesInstaller : IInstaller
    {
        private readonly string levelKey;
        private readonly ISaveStore liveSave;
        private readonly IStartingCoins startingCoins;

        public LevelTestServicesInstaller(string levelKey, ISaveStore liveSave, IStartingCoins startingCoins)
        {
            this.levelKey = levelKey;
            this.liveSave = liveSave;
            this.startingCoins = startingCoins;
        }

        public void Install(IContainerBuilder builder)
        {
            var testSave = new MemorySaveStore();
            testSave.Save(Settings.SaveKey, liveSave.Load<SettingsData>(Settings.SaveKey));

            builder.RegisterInstance<ISaveStore>(testSave);
            builder.RegisterInstance<ILevelChoice>(new FixedLevelChoice(levelKey));
            builder.RegisterInstance(startingCoins);
        }
    }
}
