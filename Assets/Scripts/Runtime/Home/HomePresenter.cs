using System;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives Home from the meta (§6): the wallet's coins, the current level on the level button and the coming ones
    /// on the level track, all set once, since nothing changes them while Home is shown. The level button opens the
    /// Play popup, Settings the Settings popup. In a level test the values are the test's own (D130).
    /// </summary>
    public sealed class HomePresenter : IDisposable
    {
        private readonly IHomeView view;
        private readonly Action openSettings;
        private readonly Action openPlay;

        public HomePresenter(IHomeView view, Wallet wallet, Progression progression, Action openSettings,
            Action openPlay)
        {
            this.view = view;
            this.openSettings = openSettings;
            this.openPlay = openPlay;

            view.SetCoins(wallet.Coins);
            view.SetLevel(progression.LevelNumber);
            view.SetComingLevels(progression.LevelNumber + 1);
            view.SettingsClicked += openSettings;
            view.LevelClicked += openPlay;
        }

        public void Dispose()
        {
            view.SettingsClicked -= openSettings;
            view.LevelClicked -= openPlay;
        }
    }
}
