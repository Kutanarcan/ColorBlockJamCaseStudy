using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay scene's scope (D64): a child of the root scope when the scene loader opens it, a scope of its own
    /// when the scene is played alone in the Editor. Closing the scene disposes it and its asset scope. It registers
    /// nothing yet: <see cref="GameplayInstaller"/> still wires the scene until I4 moves that wiring here.
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
