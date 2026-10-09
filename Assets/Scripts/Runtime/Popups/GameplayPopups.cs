using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The popups the Gameplay scene opens and what their buttons do there (D121): the Gameplay side of each popup's
    /// actions. A popup is got from the catalog on its first open, its presenter made once, and it is shown through
    /// the modal layer, which pauses play while it is up (D122); opening one while another is up replaces it (Settings
    /// → LoseLife). Lives with the Gameplay scope; closing the scene stops whatever it was opening or closing.
    /// </summary>
    public sealed class GameplayPopups : ISettingsActions, ILoseLifeActions, IDisposable
    {
        private readonly PopupService popups;
        private readonly ModalLayer modals;
        private readonly Settings settings;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private GameplayLoop play;
        private int levelNumber;
        private SettingsView settingsView;
        private SettingsPresenter settingsPresenter;
        private LoseLifeView loseLifeView;
        private LoseLifePresenter loseLifePresenter;

        public GameplayPopups(PopupService popups, ModalLayer modals, Settings settings)
        {
            this.popups = popups;
            this.modals = modals;
            this.settings = settings;
        }

        bool ISettingsActions.ShowsHome => true;

        /// <summary>The level being played, once the scene has built it: what Retry restarts and LoseLife names.</summary>
        public void PlayWith(GameplayLoop loop, int level)
        {
            play = loop;
            levelNumber = level;
        }

        /// <summary>HUD Pause (D122).</summary>
        public void OpenSettings() => OpenSettingsAsync(life.Token).Forget();

        /// <summary>HUD Restart (U3.2).</summary>
        public void OpenRetry() => OpenLoseLifeAsync(LoseLifeVariant.Retry, life.Token).Forget();

        void ISettingsActions.Home() => OpenLoseLifeAsync(LoseLifeVariant.Leave, life.Token).Forget();

        void ISettingsActions.Close() => CloseAndResume();

        void ILoseLifeActions.Retry()
        {
            // Restart resumes play itself (D71), so the close does not.
            modals.Close(resume: false).Play(life.Token).Forget();
            play?.Restart();
        }

        void ILoseLifeActions.Leave()
        {
            // M3: back to Home.
        }

        void ILoseLifeActions.Close() => CloseAndResume();

        public void Dispose()
        {
            life.Cancel();
            life.Dispose();
            settingsPresenter?.Dispose();
            loseLifePresenter?.Dispose();
        }

        private void CloseAndResume() => modals.Close(resume: true).Play(life.Token).Forget();

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
    }
}
