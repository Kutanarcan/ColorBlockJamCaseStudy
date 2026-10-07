using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public async UniTask<IReadOnlyList<string>> CheckForCatalogUpdatesAsync(CancellationToken cancellation)
        {
            List<string> catalogs = await ResultOf(Addressables.CheckForCatalogUpdates(false), cancellation);

            return catalogs == null ? new List<string>() : new List<string>(catalogs);
        }

        public UniTask UpdateCatalogsAsync(IReadOnlyList<string> catalogs, CancellationToken cancellation)
        {
            // Cleaning drops the cached bundles the new catalogs no longer use, so old versions do not pile up.
            AsyncOperationHandle<List<IResourceLocator>> handle =
                Addressables.UpdateCatalogs(autoCleanBundleCache: true, catalogs: catalogs, autoReleaseHandle: false);

            return ResultOf(handle, cancellation);
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
