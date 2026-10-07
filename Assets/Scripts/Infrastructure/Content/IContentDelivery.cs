using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// The content server's side of a content update: catalogs that changed, and content to download by label.
    /// Without a server every answer is "nothing new".
    /// </summary>
    public interface IContentDelivery
    {
        UniTask<IReadOnlyList<string>> CheckForCatalogUpdatesAsync(CancellationToken cancellation);

        UniTask UpdateCatalogsAsync(IReadOnlyList<string> catalogs, CancellationToken cancellation);

        UniTask<long> GetDownloadSizeAsync(string label, CancellationToken cancellation);

        UniTask DownloadAsync(string label, CancellationToken cancellation);
    }
}
