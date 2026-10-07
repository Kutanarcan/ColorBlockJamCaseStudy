using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public sealed class BootstrapperTests
    {
        [Test]
        public void Bootstrapper_InitializesContent_OnceOnStart()
        {
            var content = new FakeContentInitializer();

            new Bootstrapper(content).StartAsync(CancellationToken.None).Forget();

            Assert.That(content.Calls, Is.EqualTo(1));
        }
    }
}
