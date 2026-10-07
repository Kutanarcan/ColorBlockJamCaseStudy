using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>
    /// The asset loader of one <c>LifetimeScope</c> (D66): keeps every handle it loads and releases them all when the
    /// scope is disposed. A load that finishes after that would outlive its scope: it is released at once, reported
    /// as a leak in development, and the caller sees a cancel.
    /// </summary>
    public sealed class AssetScope : IAssetLoader, IDisposable
    {
        private readonly IAssetSource source;
        private readonly List<IAssetHandle> handles = new List<IAssetHandle>();
        private bool disposed;

        public AssetScope(IAssetSource source) => this.source = source;

        public int OpenHandles => handles.Count;

        public async UniTask<T> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(AssetScope));

            IAssetHandle handle = await source.LoadAsync<T>(key, cancellation);

            if (disposed)
            {
                handle.Release();
                ReportLateLoad(key);

                throw new OperationCanceledException($"Asset '{key}' finished loading after its scope closed.");
            }

            handles.Add(handle);

            return (T)handle.Asset;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            for (int i = 0; i < handles.Count; i++)
                handles[i].Release();

            handles.Clear();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private static void ReportLateLoad(string key) =>
            Debug.LogWarning($"Asset leak avoided: '{key}' finished loading after its scope closed and was released.");
    }
}
