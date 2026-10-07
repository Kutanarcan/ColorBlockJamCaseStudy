using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.Infrastructure
{
    public sealed class AssetScopeTests
    {
        private FakeAssetSource source;
        private AssetScope scope;

        [SetUp]
        public void SetUp()
        {
            source = new FakeAssetSource();
            scope = new AssetScope(source);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (FakeAssetHandle handle in source.Handles)
                Object.DestroyImmediate(handle.Asset);
        }

        [Test]
        public void AssetScope_ReleasesEveryHandle_OnDispose()
        {
            UniTask<TextAsset> first = scope.LoadAsync<TextAsset>("a", CancellationToken.None);
            UniTask<TextAsset> second = scope.LoadAsync<TextAsset>("b", CancellationToken.None);
            source.Finish("a");
            source.Finish("b");

            Assert.That(first.GetAwaiter().GetResult().text, Is.EqualTo("a"));
            Assert.That(second.GetAwaiter().GetResult().text, Is.EqualTo("b"));
            Assert.That(scope.OpenHandles, Is.EqualTo(2));

            scope.Dispose();

            Assert.That(scope.OpenHandles, Is.Zero);
            Assert.That(source.Handles, Has.All.Matches<FakeAssetHandle>(handle => handle.Releases == 1));
        }

        [Test]
        public void AssetScope_ReleasesALoadThatFinishesAfterItCloses_AndReportsIt()
        {
            UniTask<TextAsset> load = scope.LoadAsync<TextAsset>("late", CancellationToken.None);
            scope.Dispose();

            LogAssert.Expect(LogType.Warning, new Regex("'late'"));
            FakeAssetHandle handle = source.Finish("late");

            Assert.That(load.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(handle.Releases, Is.EqualTo(1));
            Assert.That(scope.OpenHandles, Is.Zero);
        }

        [Test]
        public void AssetScope_ReleasesOnlyOnce_WhenDisposedTwice()
        {
            scope.LoadAsync<TextAsset>("a", CancellationToken.None).Forget();
            FakeAssetHandle handle = source.Finish("a");

            scope.Dispose();
            scope.Dispose();

            Assert.That(handle.Releases, Is.EqualTo(1));
        }
    }
}
