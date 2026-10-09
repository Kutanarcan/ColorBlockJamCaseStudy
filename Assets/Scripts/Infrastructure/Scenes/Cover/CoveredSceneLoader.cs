using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// Puts the loading cover over every content scene change (D119), around the loader that does the change. The
    /// cover goes up before the old scene unloads and stays up for the new scene, which lifts it once built. When no
    /// scene will lift it, this lifts it: the load failed, or the content scene was only unloaded.
    /// </summary>
    public sealed class CoveredSceneLoader : ISceneLoader
    {
        private readonly ISceneLoader scenes;
        private readonly ILoadingCover cover;

        public CoveredSceneLoader(ISceneLoader scenes, ILoadingCover cover)
        {
            this.scenes = scenes;
            this.cover = cover;
        }

        public async UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation)
        {
            await cover.ShowAsync(cancellation);

            try
            {
                await scenes.ReplaceContentSceneAsync(key, cancellation);
            }
            catch
            {
                cover.Hide();
                throw;
            }
        }

        public async UniTask UnloadContentSceneAsync(CancellationToken cancellation)
        {
            await scenes.UnloadContentSceneAsync(cancellation);
            cover.Hide();
        }
    }
}
