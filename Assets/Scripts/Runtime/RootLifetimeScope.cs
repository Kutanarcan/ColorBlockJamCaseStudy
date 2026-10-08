using Game.Infrastructure;
using VContainer;

namespace Game.Runtime
{
    /// <summary>
    /// The game's root scope, in the Bootstrap scene: the live save and the level progression points to. It knows of
    /// no test mode; the Level Editor's test run starts from its own scene and scope (D131).
    /// </summary>
    public sealed class RootLifetimeScope : RootScope
    {
        protected override void InstallStartServices(IContainerBuilder builder) =>
            new LiveServicesInstaller(new JsonSaveStore(JsonSaveStore.PlayerFolder)).Install(builder);
    }
}
