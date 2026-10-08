using System;
using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public class PlayRequestTests
    {
        [Test]
        public void PlayRequest_StoresTheLevelKey_ForTheGame()
        {
            var store = new FakePlayRequestStore();

            new PlayRequest(store).Store("Level_3");

            // A new request object, as the game has after the Editor entered play mode.
            var game = new PlayRequest(store);
            Assert.That(game.TryTake(out string key), Is.True);
            Assert.That(key, Is.EqualTo("Level_3"));
            Assert.That(game.TryTake(out _), Is.False, "taken once: a later plain Play starts the scene's own level");
        }

        [Test]
        public void NoRequest_LeavesTheScenesOwnLevel()
        {
            Assert.That(new PlayRequest(new FakePlayRequestStore()).TryTake(out _), Is.False);
        }

        [Test]
        public void EmptyKey_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PlayRequest(new FakePlayRequestStore()).Store(""));
        }
    }
}
