using System;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives Home from the meta (§6): the wallet's coins, set once, since nothing on Home spends or earns; Settings
    /// opens the Settings popup. In a level test the values are the test's own (D130).
    /// </summary>
    public sealed class HomePresenter : IDisposable
    {
        private readonly HomeView view;
        private readonly Action openSettings;

        public HomePresenter(HomeView view, Wallet wallet, Action openSettings)
        {
            this.view = view;
            this.openSettings = openSettings;

            view.SetCoins(wallet.Coins);
            view.SettingsClicked += openSettings;
        }

        public void Dispose() => view.SettingsClicked -= openSettings;
    }
}
