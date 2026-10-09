using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>
    /// The run's loading cover, made from the prefab under <see cref="Key"/> (<c>Boot</c> group) by the shared root
    /// code, so the game and the level test both get it without a copy in their start scene (D135). It is made on
    /// the first show (the bootstrapper's first scene change), under the root scope's object, so it lives as long as
    /// the run (D114). Before that nothing covers the screen; the camera's background shows.
    /// </summary>
    public sealed class LoadedCover : ILoadingCover
    {
        public const string Key = "LoadingCover";

        private readonly IAssetLoader assets;
        private readonly Transform parent;
        private ILoadingCover view;

        public LoadedCover(IAssetLoader assets, Transform parent)
        {
            this.assets = assets;
            this.parent = parent;
        }

        public async UniTask ShowAsync(CancellationToken cancellation)
        {
            // A new cover starts shown, so the first show has nothing to fade.
            if (view == null)
                view = await CreateAsync(cancellation);
            else
                await view.ShowAsync(cancellation);
        }

        public void Hide() => view?.Hide();

        private async UniTask<ILoadingCover> CreateAsync(CancellationToken cancellation)
        {
            var prefab = await assets.LoadAsync<GameObject>(Key, cancellation);
            GameObject instance = Object.Instantiate(prefab, parent);

            if (!instance.TryGetComponent(out ILoadingCover cover))
                throw new InvalidOperationException($"The '{Key}' prefab has no loading cover component on its root.");

            return cover;
        }
    }
}
