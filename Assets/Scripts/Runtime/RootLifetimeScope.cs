using Game.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The root scope in the Bootstrap scene (D64): holds the services that live for the whole run. Main and Gameplay
    /// scopes become its children in I2.
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new RootInstaller(new AddressablesContentInitializer()).Install(builder);
        }
    }
}
