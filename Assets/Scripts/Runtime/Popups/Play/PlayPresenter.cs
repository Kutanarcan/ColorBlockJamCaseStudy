using System;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the Play popup (D121): in Gameplay it follows a fail, so "Level Failed!", the heart and Retry; on Home
    /// "Level X" and Play, without the heart. The place's actions are on its buttons. The booster row is static
    /// (D125). Made once per popup, shown again per open.
    /// </summary>
    public sealed class PlayPresenter : IDisposable
    {
        private const string RetryLabel = "Retry";
        private const string PlayLabel = "Play";
        private const string FailedTitle = "Level\nFailed!";

        private readonly IPlayView view;
        private readonly IPlayActions actions;

        public PlayPresenter(IPlayView view, IPlayActions actions)
        {
            this.view = view;
            this.actions = actions;
            view.ActionClicked += actions.Play;
            view.CloseClicked += actions.Close;
        }

        public void Show(int levelNumber)
        {
            // Built once per open, a popup is not a hot path.
            view.ShowTitle(actions.IsRetry ? FailedTitle : "Level " + levelNumber);
            view.ShowTitleIcon(actions.IsRetry);
            view.ShowAction(actions.IsRetry ? RetryLabel : PlayLabel);
        }

        public void Dispose()
        {
            view.ActionClicked -= actions.Play;
            view.CloseClicked -= actions.Close;
        }
    }
}
