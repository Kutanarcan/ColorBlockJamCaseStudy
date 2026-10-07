using System.Threading;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Game.Infrastructure
{
    /// <summary>
    /// The raw door to loadable content: loads by key and hands back a handle nobody tracks yet. Only
    /// <see cref="AssetScope"/> calls it (D66).
    /// </summary>
    public interface IAssetSource
    {
        UniTask<IAssetHandle> LoadAsync<T>(string key, CancellationToken cancellation) where T : Object;
    }
}
