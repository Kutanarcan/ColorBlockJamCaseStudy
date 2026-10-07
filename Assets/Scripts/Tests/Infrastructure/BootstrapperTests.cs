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

            Start(content, new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(content.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Bootstrapper_UpdatesContent_ThenLoadsGameplay_OnlyAfterContentIsReady()
        {
            var content = new FakeContentInitializer(held: true);
            var delivery = new FakeContentDelivery();
            var scenes = new FakeSceneLoader();

            Start(content, delivery, scenes);
            Assert.That(delivery.Log, Is.Empty);
            Assert.That(scenes.Log, Is.Empty);

            content.Finish();
            Assert.That(delivery.Log, Is.Not.Empty);
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        private static void Start(
            FakeContentInitializer content, FakeContentDelivery delivery, FakeSceneLoader scenes) =>
            new Bootstrapper(content, new ContentUpdate(delivery), scenes).StartAsync(CancellationToken.None).Forget();
    }
}
