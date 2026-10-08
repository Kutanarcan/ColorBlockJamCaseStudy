using System;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.Meta
{
    public sealed class LevelCompletionTests
    {
        private FakeSaveStore store;

        [SetUp]
        public void SetUp() => store = new FakeSaveStore();

        [Test]
        public void LevelCompletion_AddsTheReward_AndMovesToTheNextLevel_SavedAtOnce()
        {
            new LevelCompletion(NewWallet(), new Progression(store), 50).Record();

            Assert.That(NewWallet().Coins, Is.EqualTo(150), "A new run sees the reward.");
            Assert.That(new Progression(store).LevelNumber, Is.EqualTo(2), "A new run sees the next level.");
        }

        [Test]
        public void LevelCompletion_WithoutAReward_OnlyMovesToTheNextLevel()
        {
            Wallet wallet = NewWallet();
            int walletWrites = store.WritesOf(Wallet.SaveKey);

            new LevelCompletion(wallet, new Progression(store), 0).Record();

            Assert.That(store.WritesOf(Wallet.SaveKey), Is.EqualTo(walletWrites));
            Assert.That(new Progression(store).LevelNumber, Is.EqualTo(2));
        }

        [Test]
        public void LevelCompletion_RefusesANegativeReward()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LevelCompletion(NewWallet(), new Progression(store), -1));
        }

        private Wallet NewWallet() => new Wallet(store, new FakeStartingCoins(100));
    }
}
