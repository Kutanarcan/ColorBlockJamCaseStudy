using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class LoseLifePresenterTests
    {
        private FakeLoseLifeView view;
        private FakeLoseLifeActions actions;
        private LoseLifePresenter presenter;

        [SetUp]
        public void SetUp()
        {
            view = new FakeLoseLifeView();
            actions = new FakeLoseLifeActions();
            presenter = new LoseLifePresenter(view, actions);
        }

        [TearDown]
        public void TearDown() => presenter.Dispose();

        [Test]
        public void LoseLifePresenter_RunsItsVariantsAction()
        {
            presenter.Show(LoseLifeVariant.Retry, 3);
            view.PressAction();

            presenter.Show(LoseLifeVariant.Leave, 3);
            view.PressAction();

            Assert.That(actions.Log, Is.EqualTo(new[] { "retry", "leave" }));
        }

        [Test]
        public void LoseLifePresenter_ShowsTheLevelAndTheVariantsLabel()
        {
            presenter.Show(LoseLifeVariant.Leave, 7);

            Assert.That(view.Title, Is.EqualTo(7));
            Assert.That(view.Action, Is.EqualTo("Leave"));

            presenter.Show(LoseLifeVariant.Retry, 7);

            Assert.That(view.Action, Is.EqualTo("Retry"));
        }

        [Test]
        public void LoseLifePresenter_GoesBackToPlay_OnClose_AndStopsListeningWhenDisposed()
        {
            presenter.Show(LoseLifeVariant.Retry, 1);
            view.PressClose();
            presenter.Dispose();
            view.PressClose();
            view.PressAction();

            Assert.That(actions.Log, Is.EqualTo(new[] { "close" }));
        }
    }
}
