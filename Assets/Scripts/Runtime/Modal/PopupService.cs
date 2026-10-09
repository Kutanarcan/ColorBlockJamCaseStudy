using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    /// <summary>
    /// The popup catalog of one content scene (D134, D135): gets a popup by its key (<see cref="PopupKeys"/>) through
    /// the scene scope's asset loader and makes it once, off, under the modal layer; later calls get the same one.
    /// The caller fills it and opens it through the <see cref="ModalLayer"/>. The prefab is released with the scope
    /// (D66, D113) and the popup goes with the scene, so nothing outlives the scene that opened it. Panels are not
    /// here: they are placed in their scene.
    /// </summary>
    public sealed class PopupService
    {
        private readonly IAssetLoader assets;
        private readonly Transform layer;
        private readonly Dictionary<string, GameObject> made = new Dictionary<string, GameObject>();

        public PopupService(IAssetLoader assets, Transform layer)
        {
            this.assets = assets;
            this.layer = layer;
        }

        /// <summary>The popup under <paramref name="key"/>, by the view component on its root.</summary>
        public async UniTask<T> GetAsync<T>(string key, CancellationToken cancellation) where T : Component
        {
            if (!made.TryGetValue(key, out GameObject popup))
            {
                var prefab = await assets.LoadAsync<GameObject>(key, cancellation);

                // Another call may have made it while this one was loading.
                if (!made.TryGetValue(key, out popup))
                {
                    popup = Object.Instantiate(prefab, layer, false);
                    popup.SetActive(false);
                    made.Add(key, popup);
                }
            }

            if (!popup.TryGetComponent(out T view))
                throw new InvalidOperationException($"The '{key}' popup has no {typeof(T).Name} on its root.");

            return view;
        }
    }
}
