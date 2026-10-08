using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Infrastructure
{
    /// <summary>
    /// Asks Addressables. Every operation's handle is released once its answer is read; the catalog list is copied
    /// first, because it belongs to the operation.
    /// </summary>
    public sealed class AddressablesContentDelivery : IContentDelivery
    {
        private static readonly TimeSpan CacheReadyTimeout = TimeSpan.FromSeconds(5);

        public async UniTask<IReadOnlyList<string>> CheckForCatalogUpdatesAsync(CancellationToken cancellation)
        {
            List<string> catalogs = await ResultOf(Addressables.CheckForCatalogUpdates(false), cancellation);

            return catalogs == null ? new List<string>() : new List<string>(catalogs);
        }

        public async UniTask UpdateCatalogsAsync(IReadOnlyList<string> catalogs, CancellationToken cancellation)
        {
            await ResultOf(Addressables.UpdateCatalogs(catalogs, false), cancellation);
            await CleanBundleCacheAsync(cancellation);
        }

        public UniTask<long> GetDownloadSizeAsync(string label, CancellationToken cancellation) =>
            ResultOf(Addressables.GetDownloadSizeAsync((object)label), cancellation);

        public async UniTask DownloadAsync(string label, CancellationToken cancellation)
        {
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync((object)label);

            try
            {
                await handle.ToUniTask(cancellationToken: cancellation);
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// Drops cached bundles the new catalogs no longer use, so old versions do not pile up. It is upkeep, not part
        /// of starting the game: it waits for the bundle cache, which is ready late on Android (I6), and a failure only
        /// warns.
        /// </summary>
        private static async UniTask CleanBundleCacheAsync(CancellationToken cancellation)
        {
            try
            {
                await UniTask.WaitUntil(() => Caching.ready, cancellationToken: cancellation)
                    .Timeout(CacheReadyTimeout);
                bool cleaned = await ResultOf(Addressables.CleanBundleCache(), cancellation);

                if (!cleaned)
                    Debug.LogWarning("Old bundles stay in the cache: cleaning did not succeed.");
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Debug.LogWarning($"Old bundles stay in the cache: {exception.Message}");
            }
        }

        private static async UniTask<T> ResultOf<T>(AsyncOperationHandle<T> handle, CancellationToken cancellation)
        {
            try
            {
                return await handle.ToUniTask(cancellationToken: cancellation);
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }
}
