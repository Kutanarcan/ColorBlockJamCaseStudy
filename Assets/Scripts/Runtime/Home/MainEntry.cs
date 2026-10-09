using System;
using Game.Infrastructure;
using Game.Meta;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Main scope's entry point: Home is placed by hand and needs no loading, so it is wired and the loading cover
    /// lifts as soon as the scene starts (D119). The level button and the level track join in M2.3.
    /// </summary>
    public sealed class MainEntry : IStartable, IDisposable
    {
        private readonly ILoadingCover cover;
        private readonly HomeView view;
        private readonly Wallet wallet;
        private readonly HomePopups popups;
        private HomePresenter home;

        public MainEntry(ILoadingCover cover, HomeView view, Wallet wallet, HomePopups popups)
        {
            this.cover = cover;
            this.view = view;
            this.wallet = wallet;
            this.popups = popups;
        }

        public void Start()
        {
            home = new HomePresenter(view, wallet, popups.OpenSettings);
            cover.Hide();
        }

        public void Dispose() => home?.Dispose();
    }
}
