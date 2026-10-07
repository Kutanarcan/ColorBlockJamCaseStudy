using Game.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The root scope in the Bootstrap scene (D64): holds the services that live for the whole run. Bootstrap stays
    /// loaded, so the scope does too; content scenes open next to it and their scopes become its children (D114).
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new RootInstaller(
                new AddressablesContentInitializer(),
                new AddressablesAssetSource(),
                new AddressablesSceneLoader(this))
                .Install(builder);
        }
    }
}
