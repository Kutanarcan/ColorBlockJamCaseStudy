using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// The only way to change scenes (§4). One content scene is loaded at a time, next to Bootstrap; replacing it
    /// unloads it first. Unloading a content scene disposes its <c>LifetimeScope</c> and every asset it loaded.
    /// </summary>
    public interface ISceneLoader
    {
        UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation);

        UniTask UnloadContentSceneAsync(CancellationToken cancellation);
    }
}
