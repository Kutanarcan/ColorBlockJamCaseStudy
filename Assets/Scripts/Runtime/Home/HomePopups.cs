using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The popups Home opens and what their buttons do there (D121): the Home side of each popup's actions. Settings
    /// on Home has no Home button and its X just closes (D126); nothing is paused, Home has no play. Lives with the
    /// Main scope; closing the scene stops whatever it was opening or closing.
    /// </summary>
    public sealed class HomePopups : ISettingsActions, IDisposable
    {
        private readonly PopupService popups;
        private readonly ModalLayer modals;
        private readonly Settings settings;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private SettingsView settingsView;
        private SettingsPresenter settingsPresenter;

        public HomePopups(PopupService popups, ModalLayer modals, Settings settings)
        {
            this.popups = popups;
            this.modals = modals;
            this.settings = settings;
        }

        bool ISettingsActions.ShowsHome => false;

        /// <summary>Home's Settings button (M2.2).</summary>
        public void OpenSettings() => OpenSettingsAsync(life.Token).Forget();

        void ISettingsActions.Home()
        {
            // Not shown on Home.
        }

        void ISettingsActions.Close() => modals.Close(resume: false).Play(life.Token).Forget();

        public void Dispose()
        {
            life.Cancel();
            life.Dispose();
            settingsPresenter?.Dispose();
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
    }
}
