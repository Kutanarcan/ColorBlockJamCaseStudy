using System.Threading;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>
    /// Loads an asset by key (D65) for as long as the caller's scope lives; the caller never releases it.
    /// An unknown key throws.
    /// </summary>
    public interface IAssetLoader
    {
        UniTask<T> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object;
    }
}
