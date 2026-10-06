using Game.Core;
using Game.LevelIO;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class TextAssetLevelSourceTests
    {
        private const string Level = @"{ ""schemaVersion"": 1, ""width"": 3, ""height"": 3, ""timeLimit"": 60 }";

        private TextAsset asset;

        [SetUp]
        public void SetUp() => asset = new TextAsset(Level) { name = "level-01" };

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(asset);

        private ILevelSource NewSource() =>
            new TextAssetLevelSource(new[] { asset }, new LevelJson(ModifierCatalog.Default()));

        [Test]
        public void Load_ByAssetName_ReturnsTheLevel()
        {
            Result<LevelData> loaded = NewSource().LoadAsync("level-01").Result;

            Assert.That(loaded.IsSuccess, Is.True);
            Assert.That(loaded.Value.Width, Is.EqualTo(3));
        }

        [Test]
        public void Load_UnknownKey_Fails()
        {
            Result<LevelData> loaded = NewSource().LoadAsync("level-99").Result;

            Assert.That(loaded.IsFailure, Is.True);
            Assert.That(loaded.Error, Does.Contain("level-99"));
        }
    }
}
