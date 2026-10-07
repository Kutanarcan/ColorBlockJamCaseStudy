using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// Makes loadable content ready before anything is loaded by key (D65): the first step of the bootstrapper.
    /// </summary>
    public interface IContentInitializer
    {
        UniTask InitializeAsync(CancellationToken cancellation);
    }
}
