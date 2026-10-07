using System.Collections.Generic;
using Game.Infrastructure;
using NUnit.Framework;
using VContainer;
using VContainer.Unity;

namespace Game.Tests.Infrastructure
{
    public sealed class RootInstallerTests
    {
        [Test]
        public void RootScope_ResolvesItsServices()
        {
            var content = new FakeContentInitializer();
            var builder = new ContainerBuilder();
            new RootInstaller(content).Install(builder);

            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<IContentInitializer>(), Is.SameAs(content));
            Assert.That(container.Resolve<IReadOnlyList<IAsyncStartable>>(), Has.Some.InstanceOf<Bootstrapper>());
        }
    }
}
