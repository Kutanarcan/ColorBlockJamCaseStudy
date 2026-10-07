using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;

namespace Game.Tests.Infrastructure
{
    /// <summary>
    /// A server with the given changed catalogs and download size (none and 0 by default, like no server at all).
    /// Writes each call to a log, in call order.
    /// </summary>
    internal sealed class FakeContentDelivery : IContentDelivery
    {
        private readonly IReadOnlyList<string> changedCatalogs;
        private readonly long downloadSize;

        public FakeContentDelivery(IReadOnlyList<string> changedCatalogs = null, long downloadSize = 0)
        {
            this.changedCatalogs = changedCatalogs ?? new string[0];
            this.downloadSize = downloadSize;
        }

        public List<string> Log { get; } = new List<string>();

        public UniTask<IReadOnlyList<string>> CheckForCatalogUpdatesAsync(CancellationToken cancellation)
        {
            Log.Add("check catalogs");

            return UniTask.FromResult(changedCatalogs);
        }

        public UniTask UpdateCatalogsAsync(IReadOnlyList<string> catalogs, CancellationToken cancellation)
        {
            Log.Add("update " + string.Join(", ", catalogs));

            return UniTask.CompletedTask;
        }

        public UniTask<long> GetDownloadSizeAsync(string label, CancellationToken cancellation)
        {
            Log.Add("size " + label);

            return UniTask.FromResult(downloadSize);
        }

        public UniTask DownloadAsync(string label, CancellationToken cancellation)
        {
            Log.Add("download " + label);

            return UniTask.CompletedTask;
        }
    }
}
