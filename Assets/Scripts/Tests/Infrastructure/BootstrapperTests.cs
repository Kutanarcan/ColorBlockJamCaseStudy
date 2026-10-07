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

            new Bootstrapper(content, new FakeSceneLoader()).StartAsync(CancellationToken.None).Forget();

            Assert.That(content.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Bootstrapper_LoadsGameplay_OnlyAfterContentIsReady()
        {
            var content = new FakeContentInitializer(held: true);
            var scenes = new FakeSceneLoader();

            new Bootstrapper(content, scenes).StartAsync(CancellationToken.None).Forget();
            Assert.That(scenes.Log, Is.Empty);

            content.Finish();
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }
    }
}
