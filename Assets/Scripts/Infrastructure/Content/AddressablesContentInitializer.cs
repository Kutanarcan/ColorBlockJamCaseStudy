using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Infrastructure
{
    /// <summary>Initializes Addressables: loads the settings and the content catalog.</summary>
    public sealed class AddressablesContentInitializer : IContentInitializer
    {
        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync(false);

            try
            {
                await handle.ToUniTask(cancellationToken: cancellation);
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }
}
