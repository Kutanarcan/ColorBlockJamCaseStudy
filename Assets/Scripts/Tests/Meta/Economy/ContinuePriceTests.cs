using System;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.Meta
{
    public sealed class ContinuePriceTests
    {
        private const int BasePrice = 900;
        private const int Step = 1000;

        [Test]
        public void ContinuePrice_RisesByItsStep_AfterEachContinue_AndResetsOnALevelStart()
        {
            var wallet = new Wallet(new FakeSaveStore(), new FakeStartingCoins(10000));
            var price = new ContinuePrice(BasePrice, Step);

            Assert.That(price.TryPay(wallet), Is.True);
            Assert.That(price.Current, Is.EqualTo(1900));
            Assert.That(price.TryPay(wallet), Is.True);
            Assert.That(price.Current, Is.EqualTo(2900));
            Assert.That(wallet.Coins, Is.EqualTo(10000 - 900 - 1900));

            price.Reset();

            Assert.That(price.Current, Is.EqualTo(BasePrice));
        }

        [Test]
        public void ContinuePrice_TakesNothing_AndStaysTheSame_WhenTheWalletCannotPay()
        {
            var wallet = new Wallet(new FakeSaveStore(), new FakeStartingCoins(BasePrice - 1));
            var price = new ContinuePrice(BasePrice, Step);

            Assert.That(price.TryPay(wallet), Is.False);
            Assert.That(price.Current, Is.EqualTo(BasePrice));
            Assert.That(wallet.Coins, Is.EqualTo(BasePrice - 1));
        }

        [Test]
        public void ContinuePrice_RefusesAFreeBaseOrAFallingStep()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ContinuePrice(0, Step));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ContinuePrice(BasePrice, -1));
        }
    }
}
