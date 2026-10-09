using System;
using Game.Core;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the HUD from the session and the wallet. The level number is the one being played, set once: a win moves
    /// progression on at once (D123), but the HUD keeps showing the level that was won. The countdown and the coins are
    /// read every frame and sent to the view only when what it shows changes; services raise no change events (D132
    /// T4). What the buttons do comes from its owner: until the popups exist (U2, U3) restart restarts and pause toggles.
    /// </summary>
    public sealed class HudPresenter : IDisposable
    {
        private readonly HudView view;
        private readonly LevelSession session;
        private readonly Wallet wallet;
        private readonly Action restart;
        private readonly Action pause;
        private TimerText shownTime;
        private int shownCoins;

        public HudPresenter(HudView view, LevelSession session, Wallet wallet, int levelNumber, Action restart,
            Action pause)
        {
            this.view = view;
            this.session = session;
            this.wallet = wallet;
            this.restart = restart;
            this.pause = pause;

            view.SetLevel(levelNumber);
            ShowTime(TimerText.From(session.RemainingTime));
            ShowCoins(wallet.Coins);
            view.RestartClicked += restart;
            view.PauseClicked += pause;
        }

        public void Tick()
        {
            var time = TimerText.From(session.RemainingTime);

            if (!time.Equals(shownTime))
                ShowTime(time);

            if (wallet.Coins != shownCoins)
                ShowCoins(wallet.Coins);
        }

        public void Dispose()
        {
            view.RestartClicked -= restart;
            view.PauseClicked -= pause;
        }

        private void ShowTime(TimerText time)
        {
            shownTime = time;
            view.SetTime(time.Minutes, time.Seconds);
        }

        private void ShowCoins(int coins)
        {
            shownCoins = coins;
            view.SetCoins(coins);
        }
    }
}
