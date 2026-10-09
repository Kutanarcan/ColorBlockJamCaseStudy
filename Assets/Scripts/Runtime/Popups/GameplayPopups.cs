using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The popups the Gameplay scene opens and what their buttons do there (D121): the Gameplay side of each popup's
    /// actions. A popup is got from the catalog on its first open, its presenter made once, and it is shown through
    /// the modal layer, which pauses play while it is up (D122); opening one while another is up replaces it
    /// (Settings → LoseLife, LevelFail → Play). Leave and the Play popup's X go Home (M3.1). Lives with the Gameplay
    /// scope; closing the scene stops whatever it was opening or closing.
    /// </summary>
    public sealed class GameplayPopups : ISettingsActions, ILoseLifeActions, IPlayActions, IDisposable
    {
        private readonly PopupService popups;
        private readonly ModalLayer modals;
        private readonly Settings settings;
        private readonly Navigation navigation;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private GameplayLoop play;
        private int levelNumber;
        private SettingsView settingsView;
        private SettingsPresenter settingsPresenter;
        private LoseLifeView loseLifeView;
        private LoseLifePresenter loseLifePresenter;
        private PlayView playView;
        private PlayPresenter playPresenter;

        public GameplayPopups(PopupService popups, ModalLayer modals, Settings settings, Navigation navigation)
        {
            this.popups = popups;
            this.modals = modals;
            this.settings = settings;
            this.navigation = navigation;
        }

        bool ISettingsActions.ShowsHome => true;

        bool IPlayActions.IsRetry => true;

        /// <summary>The level being played, once built: what Retry restarts and the popups name.</summary>
        public void PlayWith(GameplayLoop loop, int level)
        {
            play = loop;
            levelNumber = level;
        }

        /// <summary>HUD Pause (D122).</summary>
        public void OpenSettings() => OpenSettingsAsync(life.Token).Forget();

        /// <summary>HUD Restart (U3.2).</summary>
        public void OpenRetry() => OpenLoseLifeAsync(LoseLifeVariant.Retry, life.Token).Forget();

        /// <summary>LevelFail's X: the Play popup (Retry) takes the panel's place (U5.4).</summary>
        public void OpenPlay() => OpenPlayAsync(life.Token).Forget();

        void ISettingsActions.Home() => OpenLoseLifeAsync(LoseLifeVariant.Leave, life.Token).Forget();

        void ISettingsActions.Close() => CloseAndResume();

        void ILoseLifeActions.Retry() => RetryLevel();

        // Home under the cover (M3.1); the popup stays up until the scene is gone.
        void ILoseLifeActions.Leave() => navigation.GoHome();

        void ILoseLifeActions.Close() => CloseAndResume();

        void IPlayActions.Play() => RetryLevel();

        void IPlayActions.Close() => navigation.GoHome();

        public void Dispose()
        {
            life.Cancel();
            life.Dispose();
            settingsPresenter?.Dispose();
            loseLifePresenter?.Dispose();
            playPresenter?.Dispose();
        }

        private void CloseAndResume() => modals.Close(resume: true).Play(life.Token).Forget();

        private void RetryLevel()
        {
            // Restart resumes play itself (D71), so the close does not.
            modals.Close(resume: false).Play(life.Token).Forget();
            play?.Restart();
        }

        private async UniTask OpenSettingsAsync(CancellationToken cancellation)
        {
            if (settingsView == null)
            {
                settingsView = await popups.GetAsync<SettingsView>(PopupKeys.Settings, cancellation);
                settingsPresenter ??= new SettingsPresenter(settingsView, settings, this);
            }

            settingsPresenter.Refresh();
            await modals.Open((RectTransform)settingsView.transform).Play(cancellation);
        }

        private async UniTask OpenLoseLifeAsync(LoseLifeVariant variant, CancellationToken cancellation)
        {
            if (loseLifeView == null)
            {
                loseLifeView = await popups.GetAsync<LoseLifeView>(PopupKeys.LoseLife, cancellation);
                loseLifePresenter ??= new LoseLifePresenter(loseLifeView, this);
            }

            loseLifePresenter.Show(variant, levelNumber);
            await modals.Open((RectTransform)loseLifeView.transform).Play(cancellation);
        }

        private async UniTask OpenPlayAsync(CancellationToken cancellation)
        {
            if (playView == null)
            {
                playView = await popups.GetAsync<PlayView>(PopupKeys.Play, cancellation);
                playPresenter ??= new PlayPresenter(playView, this);
            }

            playPresenter.Show(levelNumber);
            await modals.Open((RectTransform)playView.transform).Play(cancellation);
        }
    }
}
