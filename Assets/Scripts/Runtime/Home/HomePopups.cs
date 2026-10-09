using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The popups Home opens and what their buttons do there (D121): the Home side of each popup's actions. Settings
    /// on Home has no Home button and its X just closes (D126); Play shows "Level X" without the heart and starts the
    /// level, its X closes. Nothing is paused, Home has no play. Lives with the Main scope; closing the scene stops
    /// whatever it was opening or closing.
    /// </summary>
    public sealed class HomePopups : ISettingsActions, IPlayActions, IDisposable
    {
        private readonly PopupService popups;
        private readonly ModalLayer modals;
        private readonly Settings settings;
        private readonly Progression progression;
        private readonly LevelLauncher launcher;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private SettingsView settingsView;
        private SettingsPresenter settingsPresenter;
        private PlayView playView;
        private PlayPresenter playPresenter;

        public HomePopups(PopupService popups, ModalLayer modals, Settings settings, Progression progression,
            LevelLauncher launcher)
        {
            this.popups = popups;
            this.modals = modals;
            this.settings = settings;
            this.progression = progression;
            this.launcher = launcher;
        }

        bool ISettingsActions.ShowsHome => false;

        bool IPlayActions.IsRetry => false;

        /// <summary>Home's Settings button (M2.2).</summary>
        public void OpenSettings() => OpenSettingsAsync(life.Token).Forget();

        /// <summary>Home's level button (M2.3).</summary>
        public void OpenPlay() => OpenPlayAsync(life.Token).Forget();

        void ISettingsActions.Home()
        {
            // Not shown on Home.
        }

        void ISettingsActions.Close() => CloseModal();

        // The popup stays up under the cover while the scene changes.
        void IPlayActions.Play() => launcher.Launch();

        void IPlayActions.Close() => CloseModal();

        public void Dispose()
        {
            life.Cancel();
            life.Dispose();
            settingsPresenter?.Dispose();
            playPresenter?.Dispose();
        }

        private void CloseModal() => modals.Close(resume: false).Play(life.Token).Forget();

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

        private async UniTask OpenPlayAsync(CancellationToken cancellation)
        {
            if (playView == null)
            {
                playView = await popups.GetAsync<PlayView>(PopupKeys.Play, cancellation);
                playPresenter ??= new PlayPresenter(playView, this);
            }

            playPresenter.Show(progression.LevelNumber);
            await modals.Open((RectTransform)playView.transform).Play(cancellation);
        }
    }
}
