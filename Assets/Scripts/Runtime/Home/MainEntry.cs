using Game.Infrastructure;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Main scope's entry point: Home is placed by hand and needs no loading, so the scene is ready as soon as it
    /// starts and lifts the loading cover (D119). Home's screen, popups and tabs join in M2.2–M2.4.
    /// </summary>
    public sealed class MainEntry : IStartable
    {
        private readonly ILoadingCover cover;

        public MainEntry(ILoadingCover cover) => this.cover = cover;

        public void Start() => cover.Hide();
    }
}
