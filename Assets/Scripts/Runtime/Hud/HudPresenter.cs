using System;
using Game.Core;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the HUD from the session and the wallet. The level number is the one being played, set once: a win moves
    /// progression on at once (D123), but the HUD keeps showing the level that was won. The countdown and the coins are
    /// read every frame and sent to the view only when what it shows changes; services raise no change events (D132
    /// T4). Each time the countdown's number changes while the level is played, the timer beats if time is short, and
    /// flashes red in the last seconds (<see cref="TimerAlarm"/>, D143). What the buttons do comes from its owner:
    /// pause opens Settings (U2.4), restart opens LoseLife Retry (U3).
    /// The controls work only while the level is being played: from the moment it is won or failed (the last block
    /// entering its door) until a restart or a continue, no HUD button takes a touch.
    /// </summary>
    public sealed class HudPresenter : IDisposable
    {
        private readonly HudView view;
        private readonly LevelSession session;
        private readonly Wallet wallet;
        private readonly TimerAlarm alarm;
        private readonly Action restart;
        private readonly Action pause;
        private TimerText shownTime;
        private int shownCoins;
        private bool controlsOn;

        public HudPresenter(HudView view, LevelSession session, Wallet wallet, int levelNumber, TimerAlarm alarm,
            Action restart, Action pause)
        {
            this.view = view;
            this.session = session;
            this.wallet = wallet;
            this.alarm = alarm;
            this.restart = restart;
            this.pause = pause;

            view.SetLevel(levelNumber);
            ShowTime(TimerText.From(session.RemainingTime));
            ShowCoins(wallet.Coins);
            ShowControls(session.State == GameState.Playing);
            view.RestartClicked += restart;
            view.PauseClicked += pause;
        }

        public void Tick()
        {
            var time = TimerText.From(session.RemainingTime);

            if (!time.Equals(shownTime))
            {
                ShowTime(time);
                Beat(time);
            }

            if (wallet.Coins != shownCoins)
                ShowCoins(wallet.Coins);

            bool playing = session.State == GameState.Playing;

            if (playing != controlsOn)
                ShowControls(playing);
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

        private void Beat(TimerText time)
        {
            if (session.State != GameState.Playing)
                return;

            TimerUrgency urgency = alarm.UrgencyOf(time);

            if (urgency != TimerUrgency.None)
                view.BeatTimer(urgency == TimerUrgency.Critical);
        }

        private void ShowCoins(int coins)
        {
            shownCoins = coins;
            view.SetCoins(coins);
        }

        private void ShowControls(bool on)
        {
            controlsOn = on;
            view.SetControlsOn(on);
        }
    }
}
