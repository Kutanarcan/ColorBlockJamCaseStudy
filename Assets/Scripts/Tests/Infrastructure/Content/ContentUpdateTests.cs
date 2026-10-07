using System.Threading;
using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public sealed class ContentUpdateTests
    {
        [Test]
        public void ContentUpdate_RunsEveryStep_WithNothingToDownload()
        {
            var delivery = new FakeContentDelivery();

            long downloaded = new ContentUpdate(delivery).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(delivery.Log, Is.EqualTo(new[] { "check catalogs", "size remote", "download remote" }));
            Assert.That(downloaded, Is.Zero);
        }

        [Test]
        public void ContentUpdate_UpdatesTheChangedCatalogs_BeforeSizingTheDownload()
        {
            var delivery = new FakeContentDelivery(new[] { "catalog_a" }, downloadSize: 2048);

            long downloaded = new ContentUpdate(delivery).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(delivery.Log,
                Is.EqualTo(new[] { "check catalogs", "update catalog_a", "size remote", "download remote" }));
            Assert.That(downloaded, Is.EqualTo(2048));
        }
    }
}
