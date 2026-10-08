using System;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.Meta
{
    public sealed class WalletTests
    {
        private const int StartCoins = 1000;

        private FakeSaveStore store;

        [SetUp]
        public void SetUp() => store = new FakeSaveStore();

        [Test]
        public void Coins_StayCorrect_AfterLeavingAndReturning()
        {
            Wallet wallet = NewRun();
            wallet.Add(50);
            wallet.TrySpend(900);
            new LevelCompletion(wallet, new Progression(store), 50).Record();

            Assert.That(wallet.Coins, Is.EqualTo(StartCoins + 50 - 900 + 50));
            Assert.That(NewRun().Coins, Is.EqualTo(wallet.Coins), "The next run reads the same balance.");
        }

        [Test]
        public void Wallet_GivesTheStartingCoins_OnlyToAFreshPlayer()
        {
            Wallet wallet = NewRun();
            Assert.That(wallet.Coins, Is.EqualTo(StartCoins));

            wallet.TrySpend(StartCoins);

            Assert.That(NewRun().Coins, Is.Zero, "Spending everything does not bring the starting coins back.");
        }

        [Test]
        public void Wallet_RefusesToSpend_WhatItDoesNotHave_AndKeepsItsBalance()
        {
            Wallet wallet = NewRun();

            Assert.That(wallet.CanAfford(StartCoins + 1), Is.False);
            Assert.That(wallet.TrySpend(StartCoins + 1), Is.False);
            Assert.That(wallet.Coins, Is.EqualTo(StartCoins));
            Assert.That(wallet.TrySpend(StartCoins), Is.True, "Exactly the balance is covered.");
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Wallet_RefusesAmountsThatAreNotPositive(int amount)
        {
            Wallet wallet = NewRun();

            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.TrySpend(amount));
        }

        private Wallet NewRun() => new Wallet(store, new FakeStartingCoins(StartCoins));
    }
}
