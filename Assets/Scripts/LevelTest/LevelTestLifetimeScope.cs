using System;
using Game.Infrastructure;
using Game.Runtime;
using VContainer;

namespace Game.LevelTest
{
    /// <summary>
    /// The level test's root scope, in the Editor-only LevelTest scene (D131). It runs the game's own start-up flow and
    /// systems, but on the level test's services; the game's Bootstrap scene and scope never hear of it.
    /// </summary>
    public sealed class LevelTestLifetimeScope : RootScope
    {
        protected override void InstallStartServices(IContainerBuilder builder)
        {
            if (!new PlayRequest(new EditorPlayRequestStore()).TryTake(out string levelKey))
                throw new InvalidOperationException(
                    "The LevelTest scene was played without a level; start it from the Level Editor's ▶ Play.");

            new LevelTestServicesInstaller(levelKey, new JsonSaveStore(JsonSaveStore.PlayerFolder)).Install(builder);
        }
    }
}
