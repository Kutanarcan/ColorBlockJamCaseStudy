using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// The path a server build takes at start-up (D65): new catalogs → download size → download. It always runs;
    /// locally it finds no catalog and nothing to download.
    /// </summary>
    public sealed class ContentUpdate
    {
        private readonly IContentDelivery delivery;

        public ContentUpdate(IContentDelivery delivery) => this.delivery = delivery;

        /// <returns>The bytes the download had to fetch.</returns>
        public async UniTask<long> RunAsync(CancellationToken cancellation)
        {
            IReadOnlyList<string> catalogs = await delivery.CheckForCatalogUpdatesAsync(cancellation);

            if (catalogs.Count > 0)
                await delivery.UpdateCatalogsAsync(catalogs, cancellation);

            long size = await delivery.GetDownloadSizeAsync(ContentLabels.Remote, cancellation);
            await delivery.DownloadAsync(ContentLabels.Remote, cancellation);

            return size;
        }
    }
}
