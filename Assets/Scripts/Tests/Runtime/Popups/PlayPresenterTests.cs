using System;
using System.Collections.Generic;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class PlayPresenterTests
    {
        [Test]
        public void PlayPresenter_InGameplay_ShowsTheFail_RetriesWithTheHeart_AndClosesToItsPlace()
        {
            var view = new View();
            var actions = new Actions(isRetry: true);
            using var presenter = new PlayPresenter(view, actions);

            presenter.Show(4);
            view.ActionClicked?.Invoke();
            view.CloseClicked?.Invoke();

            Assert.That(view.Title, Is.EqualTo("Level\nFailed!"), "it follows a fail");
            Assert.That(view.HeartShown, Is.True);
            Assert.That(view.Label, Is.EqualTo("Retry"));
            Assert.That(actions.Log, Is.EqualTo(new[] { "play", "close" }));
        }

        [Test]
        public void PlayPresenter_OnHome_ShowsTheLevel_AndPlaysWithoutTheHeart()
        {
            var view = new View();
            using var presenter = new PlayPresenter(view, new Actions(isRetry: false));

            presenter.Show(4);

            Assert.That(view.Title, Is.EqualTo("Level 4"));
            Assert.That(view.HeartShown, Is.False);
            Assert.That(view.Label, Is.EqualTo("Play"));
        }

        /// <summary>Remembers what it shows; the test raises its buttons.</summary>
        private sealed class View : IPlayView
        {
            public Action ActionClicked;
            public Action CloseClicked;

            event Action IPlayView.ActionClicked
            {
                add => ActionClicked += value;
                remove => ActionClicked -= value;
            }

            event Action IPlayView.CloseClicked
            {
                add => CloseClicked += value;
                remove => CloseClicked -= value;
            }

            public string Title { get; private set; }

            public bool HeartShown { get; private set; }

            public string Label { get; private set; }

            public void ShowTitle(string title) => Title = title;

            public void ShowTitleIcon(bool shown) => HeartShown = shown;

            public void ShowAction(string label) => Label = label;
        }

        /// <summary>Does nothing; writes "play" and "close" to a log.</summary>
        private sealed class Actions : IPlayActions
        {
            public Actions(bool isRetry) => IsRetry = isRetry;

            public bool IsRetry { get; }

            public List<string> Log { get; } = new List<string>();

            public void Play() => Log.Add("play");

            public void Close() => Log.Add("close");
        }
    }
}
