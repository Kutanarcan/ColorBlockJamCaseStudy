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
    /// the modal layer, which pauses play while it is up (D122). Lives with the Gameplay scope; closing the scene
    /// stops whatever it was opening or closing.
    /// </summary>
    public sealed class GameplayPopups : ISettingsActions, IDisposable
    {
        private readonly PopupService popups;
        private readonly ModalLayer modals;
        private readonly Settings settings;
        private readonly CancellationTokenSource life = new CancellationTokenSource();
        private SettingsView settingsView;
        private SettingsPresenter settingsPresenter;

        public GameplayPopups(PopupService popups, ModalLayer modals, Settings settings)
        {
            this.popups = popups;
            this.modals = modals;
            this.settings = settings;
        }

        bool ISettingsActions.ShowsHome => true;

        /// <summary>HUD Pause (D122).</summary>
        public void OpenSettings() => OpenSettingsAsync(life.Token).Forget();

        void ISettingsActions.Home()
        {
            // U3.3: Home → LoseLife (Leave).
        }

        void ISettingsActions.Close() => modals.Close(resume: true).Play(life.Token).Forget();

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
