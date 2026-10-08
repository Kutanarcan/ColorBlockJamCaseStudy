using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Infrastructure
{
    public sealed class BootstrapperTests
    {
        private GameConfig config;
        private FakeAssetLoader assets;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            assets = new FakeAssetLoader().Add(GameConfig.Key, config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void Bootstrapper_InitializesContent_OnceOnStart()
        {
            var content = new FakeContentInitializer();

            Start(content, new FakeContentDelivery(), new LoadedConfig(), new FakeSceneLoader());

            Assert.That(content.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Bootstrapper_UpdatesContent_LoadsConfig_ThenGameplay_OnlyAfterContentIsReady()
        {
            var content = new FakeContentInitializer(held: true);
            var delivery = new FakeContentDelivery();
            var loaded = new LoadedConfig();
            var scenes = new FakeSceneLoader();

            Start(content, delivery, loaded, scenes);
            Assert.That(delivery.Log, Is.Empty);
            Assert.That(scenes.Log, Is.Empty);

            content.Finish();
            Assert.That(delivery.Log, Is.Not.Empty);
            Assert.That(loaded.Value, Is.SameAs(config));
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        private void Start(
            FakeContentInitializer content, FakeContentDelivery delivery, LoadedConfig loaded,
            FakeSceneLoader scenes) =>
            new Bootstrapper(content, new ContentUpdate(delivery), assets, loaded, scenes)
                .StartAsync(CancellationToken.None)
                .Forget();
    }
}
