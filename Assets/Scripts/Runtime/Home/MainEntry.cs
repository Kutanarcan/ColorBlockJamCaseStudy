using System;
using Game.Infrastructure;
using Game.Meta;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Main scope's entry point: Home is placed by hand and needs no loading, so it is wired and the loading cover
    /// lifts as soon as the scene starts (D119).
    /// </summary>
    public sealed class MainEntry : IStartable, IDisposable
    {
        private readonly ILoadingCover cover;
        private readonly HomeView view;
        private readonly Wallet wallet;
        private readonly Progression progression;
        private readonly HomePopups popups;
        private HomePresenter home;

        public MainEntry(ILoadingCover cover, HomeView view, Wallet wallet, Progression progression, HomePopups popups)
        {
            this.cover = cover;
            this.view = view;
            this.wallet = wallet;
            this.progression = progression;
            this.popups = popups;
        }

        public void Start()
        {
            home = new HomePresenter(view, wallet, progression, popups.OpenSettings, popups.OpenPlay);
            cover.Hide();
        }

        public void Dispose() => home?.Dispose();
    }
}
