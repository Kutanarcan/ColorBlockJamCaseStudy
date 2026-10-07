using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// The root scope's registrations, kept out of the <c>LifetimeScope</c> so a test can build them without a scene.
    /// </summary>
    public sealed class RootInstaller : IInstaller
    {
        private readonly IContentInitializer content;

        public RootInstaller(IContentInitializer content) => this.content = content;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(content);
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
