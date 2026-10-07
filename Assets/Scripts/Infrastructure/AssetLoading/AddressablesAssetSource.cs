using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>
    /// Loads from Addressables. A load that fails or is cancelled releases its own handle, so only a loaded asset
    /// reaches the scope.
    /// </summary>
    public sealed class AddressablesAssetSource : IAssetSource
    {
        public async UniTask<IAssetHandle> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);

            try
            {
                await handle.ToUniTask(cancellationToken: cancellation, autoReleaseWhenCanceled: true);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Addressables.Release(handle);

                throw;
            }

            return new Handle(handle);
        }

        private sealed class Handle : IAssetHandle
        {
            private readonly AsyncOperationHandle handle;

            public Handle(AsyncOperationHandle handle) => this.handle = handle;

            public Object Asset => (Object)handle.Result;

            public void Release() => Addressables.Release(handle);
        }
    }
}
