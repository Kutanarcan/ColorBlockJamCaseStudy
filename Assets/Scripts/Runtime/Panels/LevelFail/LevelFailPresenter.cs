using System;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the LevelFail panel (D124): the fail kind's content, the continue price and the coins. Continue pays the
    /// price from the wallet and plays on; when the wallet cannot pay, the price shows red and the press does nothing.
    /// The price rises after each continue and goes back to its base at every level start (<see cref="ContinuePrice"/>).
    /// </summary>
    public sealed class LevelFailPresenter : IDisposable
    {
        private readonly ILevelFailView view;
        private readonly Wallet wallet;
        private readonly ContinuePrice price;
        private readonly int seconds;
        private readonly ILevelFailActions actions;

        public LevelFailPresenter(ILevelFailView view, Wallet wallet, ContinuePrice price, int seconds,
            ILevelFailActions actions)
        {
            this.view = view;
            this.wallet = wallet;
            this.price = price;
            this.seconds = seconds;
            this.actions = actions;
            view.ContinueClicked += Continue;
            view.CloseClicked += actions.Close;
        }

        public void Show(FailContent content)
        {
            view.ShowContent(content.Title, content.Icon, seconds, content.Description);
            ShowWallet();
        }

        public void Dispose()
        {
            view.ContinueClicked -= Continue;
            view.CloseClicked -= actions.Close;
        }

        private void Continue()
        {
            if (!price.TryPay(wallet))
                return;

            // The panel closes as it is; the new price and coins show the next time it opens.
            actions.Continue(seconds);
        }

        private void ShowWallet()
        {
            view.ShowPrice(price.Current, wallet.CanAfford(price.Current));
            view.ShowCoins(wallet.Coins);
        }
    }
}
