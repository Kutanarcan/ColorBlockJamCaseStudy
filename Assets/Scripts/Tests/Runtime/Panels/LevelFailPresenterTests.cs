using Game.Meta;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class LevelFailPresenterTests
    {
        private FakeLevelFailView view;
        private FakeLevelFailActions actions;
        private Wallet wallet;
        private LevelFailPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            view = new FakeLevelFailView();
            actions = new FakeLevelFailActions();
            wallet = new TestMeta(startCoins: 1000).Wallet;
            presenter = new LevelFailPresenter(view, wallet, new ContinuePrice(900, 1000), 20, actions);
        }

        [TearDown]
        public void TearDown() => presenter.Dispose();

        [Test]
        public void FailPresenter_Continues_OnlyWhenTheWalletCanPay()
        {
            presenter.Show(new FailContent());
            Assert.That(view.Price, Is.EqualTo(900));
            Assert.That(view.Affordable, Is.True);

            view.PressContinue();
            Assert.That(actions.Log, Is.EqualTo(new[] { "continue 20" }), "paid: the level gets its seconds");
            Assert.That(wallet.Coins, Is.EqualTo(100));

            presenter.Show(new FailContent());
            Assert.That(view.Price, Is.EqualTo(1900), "each continue is dearer");
            Assert.That(view.Affordable, Is.False, "the price shows red");

            view.PressContinue();
            Assert.That(actions.Log, Is.EqualTo(new[] { "continue 20" }), "too dear: the press does nothing");
            Assert.That(wallet.Coins, Is.EqualTo(100));
        }

        [Test]
        public void FailPresenter_SellsOneContinue_PerShowing()
        {
            wallet.Add(5000);
            presenter.Show(new FailContent());

            view.PressContinue();
            view.PressContinue();

            Assert.That(actions.Log, Is.EqualTo(new[] { "continue 20" }), "the second press lands while the panel closes");
            Assert.That(wallet.Coins, Is.EqualTo(5100), "paid once: 6000 - 900");

            presenter.Show(new FailContent());
            view.PressContinue();
            Assert.That(actions.Log, Is.EqualTo(new[] { "continue 20", "continue 20" }), "the next showing sells again");
        }

        [Test]
        public void FailPresenter_ShowsTheFailKind_TheBonusAndTheCoins()
        {
            presenter.Show(new FailContent());

            Assert.That(view.Title, Is.EqualTo("Out of Time!"));
            Assert.That(view.Bonus, Is.EqualTo(20));
            Assert.That(view.Coins, Is.EqualTo(1000));
        }

        [Test]
        public void FailPresenter_ShowsTheBoard_WhileTheHoldAreaIsHeld()
        {
            view.Hold();
            view.Release();

            Assert.That(actions.Log, Is.EqualTo(new[] { "see board on", "see board off" }));
        }

        [Test]
        public void FailPresenter_HandsTheCloseToItsPlace()
        {
            view.PressClose();

            Assert.That(actions.Log, Is.EqualTo(new[] { "close" }));
        }
    }
}
