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
        private LoadedConfig loaded;
        private FakePlayRequestStore requests;
        private SelectedLevel level;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            loaded = new LoadedConfig(new FakeAssetLoader().Add(GameConfig.Key, config));
            requests = new FakePlayRequestStore();
            level = new SelectedLevel(loaded, new PlayRequest(requests));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void Bootstrapper_InitializesContent_OnceOnStart()
        {
            var content = new FakeContentInitializer();

            Start(content, new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(content.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Bootstrapper_UpdatesContent_LoadsConfig_ThenGameplay_OnlyAfterContentIsReady()
        {
            var content = new FakeContentInitializer(held: true);
            var delivery = new FakeContentDelivery();
            var scenes = new FakeSceneLoader();

            Start(content, delivery, scenes);
            Assert.That(delivery.Log, Is.Empty);
            Assert.That(scenes.Log, Is.Empty);

            content.Finish();
            Assert.That(delivery.Log, Is.Not.Empty);
            Assert.That(loaded.Value, Is.SameAs(config));
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        [Test]
        public void Bootstrapper_OpensTheRequestedLevel_WhenOneIsStored()
        {
            new PlayRequest(requests).Store("Level_3");

            Start(new FakeContentInitializer(), new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(level.Key, Is.EqualTo("Level_3"));
            Assert.That(requests.Value, Is.Empty, "The request is taken once.");
        }

        [Test]
        public void Bootstrapper_OpensTheFirstConfiguredLevel_WithoutARequest()
        {
            Start(new FakeContentInitializer(), new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(level.Key, Is.EqualTo(config.LevelKeys[0]));
        }

        private void Start(FakeContentInitializer content, FakeContentDelivery delivery, FakeSceneLoader scenes) =>
            new Bootstrapper(content, new ContentUpdate(delivery), loaded, level, scenes)
                .StartAsync(CancellationToken.None)
                .Forget();
    }
}
