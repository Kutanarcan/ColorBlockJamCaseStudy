using System.Threading;
using Game.Infrastructure;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public sealed class PopupServiceTests
    {
        private GameObject prefab;
        private GameObject layer;
        private FakePopupSource source;
        private AssetScope scope;
        private PopupService popups;

        [SetUp]
        public void SetUp()
        {
            prefab = new GameObject("SettingsPopup", typeof(RectTransform));
            layer = new GameObject("ModalLayer", typeof(RectTransform));
            source = new FakePopupSource(prefab);
            scope = new AssetScope(source);
            popups = new PopupService(scope, layer.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(layer);
        }

        [Test]
        public void PopupService_MakesThePopupOnce_OffUnderTheModalLayer()
        {
            RectTransform first = Get();
            RectTransform again = Get();

            Assert.That(again, Is.SameAs(first));
            Assert.That(source.Loads, Is.EqualTo(1));
            Assert.That(first.parent, Is.SameAs(layer.transform));
            Assert.That(first.gameObject.activeSelf, Is.False, "The modal layer turns it on when it opens.");
        }

        [Test]
        public void PopupService_ReleasesThePopup_WhenItsScopeCloses()
        {
            Get();

            scope.Dispose();

            Assert.That(source.Releases, Is.EqualTo(1));
        }

        private RectTransform Get() =>
            popups.GetAsync<RectTransform>(PopupKeys.Settings, CancellationToken.None).GetAwaiter().GetResult();
    }
}
