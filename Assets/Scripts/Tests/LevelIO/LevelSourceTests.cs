using Game.Core;
using Game.LevelIO;
using NUnit.Framework;

namespace Game.Tests.LevelIO
{
    public class LevelSourceTests
    {
        private const string Level = @"{ ""schemaVersion"": 1, ""width"": 3, ""height"": 3, ""timeLimit"": 60 }";

        private static ILevelSource NewSource() =>
            new FakeLevelSource(new LevelJson(ModifierCatalog.Default())).Add("level-01", Level);

        [Test]
        public void Load_KnownKey_ReturnsTheLevel()
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
