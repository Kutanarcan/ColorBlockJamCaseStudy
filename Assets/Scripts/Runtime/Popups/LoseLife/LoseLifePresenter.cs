using System;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the LoseLife popup (D121): "Level X" and the variant's action. The action button does what the variant
    /// asks (retry or leave); X goes back to play. Made once per popup and shown again for each open.
    /// </summary>
    public sealed class LoseLifePresenter : IDisposable
    {
        private const string RetryLabel = "Retry";
        private const string LeaveLabel = "Leave";

        private readonly ILoseLifeView view;
        private readonly ILoseLifeActions actions;
        private LoseLifeVariant variant;

        public LoseLifePresenter(ILoseLifeView view, ILoseLifeActions actions)
        {
            this.view = view;
            this.actions = actions;
            view.ActionClicked += RunAction;
            view.CloseClicked += actions.Close;
        }

        public void Show(LoseLifeVariant shown, int levelNumber)
        {
            variant = shown;
            view.ShowTitle(levelNumber);
            view.ShowAction(shown == LoseLifeVariant.Retry ? RetryLabel : LeaveLabel);
        }

        public void Dispose()
        {
            view.ActionClicked -= RunAction;
            view.CloseClicked -= actions.Close;
        }

        private void RunAction()
        {
            if (variant == LoseLifeVariant.Retry)
                actions.Retry();
            else
                actions.Leave();
        }
    }
}
