using System;
using System.Threading;
using Game.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests.Infrastructure
{
    public sealed class LoadedConfigTests
    {
        private GameConfig config;
        private LoadedConfig loaded;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            loaded = new LoadedConfig(new FakeAssetLoader().Add(GameConfig.Key, config));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void LoadedConfig_Throws_WhenReadBeforeItIsLoaded()
        {
            Assert.Throws<InvalidOperationException>(() => _ = loaded.Value);
        }

        [Test]
        public void LoadedConfig_LoadsByItsKey_OncePerRun()
        {
            loaded.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded.Value, Is.SameAs(config));
            Assert.Throws<InvalidOperationException>(
                () => loaded.LoadAsync(CancellationToken.None).GetAwaiter().GetResult());
        }
    }
}
