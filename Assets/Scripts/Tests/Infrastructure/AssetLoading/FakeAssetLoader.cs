using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Object = UnityEngine.Object;

namespace Game.Tests.Infrastructure
{
    /// <summary>
    /// Assets by key, ready at once. An unknown key throws, as Addressables does. The test owns the assets and
    /// destroys them.
    /// </summary>
    internal sealed class FakeAssetLoader : IAssetLoader
    {
        private readonly Dictionary<string, Object> assets = new Dictionary<string, Object>();

        public FakeAssetLoader Add(string key, Object asset)
        {
            assets.Add(key, asset);

            return this;
        }

        public UniTask<T> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object
        {
            if (!assets.TryGetValue(key, out Object asset))
                throw new InvalidOperationException($"Unknown key '{key}'.");

            return UniTask.FromResult((T)asset);
        }
    }
}
