using Game.Core;
using Game.Infrastructure;
using Game.LevelIO;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Infrastructure
{
    public sealed class AssetLevelSourceTests
    {
        private const string Level = @"{ ""schemaVersion"": 2, ""width"": 3, ""height"": 3, ""timeLimit"": 60 }";

        private TextAsset asset;
        private ILevelSource source;

        [SetUp]
        public void SetUp()
        {
            asset = new TextAsset(Level);
            source = new AssetLevelSource(new FakeAssetLoader().Add("Level_1", asset),
                new LevelJson(ModifierCatalog.Default()));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(asset);

        [Test]
        public void LevelSource_LoadsByKey_AndRejectsAnUnknownKey()
        {
            Result<LevelData> known = source.LoadAsync("Level_1").Result;
            Result<LevelData> unknown = source.LoadAsync("Level_9").Result;

            Assert.That(known.IsSuccess, Is.True);
            Assert.That(known.Value.Width, Is.EqualTo(3));
            Assert.That(unknown.IsFailure, Is.True);
            Assert.That(unknown.Error, Does.Contain("Level_9"));
        }
    }
}
