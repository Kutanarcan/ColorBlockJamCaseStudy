using System;
using Game.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests.Infrastructure
{
    public sealed class LoadedConfigTests
    {
        private GameConfig config;

        [SetUp]
        public void SetUp() => config = ScriptableObject.CreateInstance<GameConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void LoadedConfig_Throws_WhenReadBeforeItIsSet()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new LoadedConfig().Value);
        }

        [Test]
        public void LoadedConfig_IsSetOncePerRun()
        {
            var loaded = new LoadedConfig();
            loaded.Set(config);

            Assert.That(loaded.Value, Is.SameAs(config));
            Assert.Throws<InvalidOperationException>(() => loaded.Set(config));
        }
    }
}
