using System.Threading;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public sealed class ModalLayerTests
    {
        private FakeModalLayerView view;
        private ModalLayer layer;
        private RectTransform first;
        private RectTransform second;

        [SetUp]
        public void SetUp()
        {
            view = new FakeModalLayerView();
            layer = new ModalLayer(view);
            first = (RectTransform)new GameObject("A", typeof(RectTransform)).transform;
            second = (RectTransform)new GameObject("B", typeof(RectTransform)).transform;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void ModalLayer_ShowsOneAtATime_TheNextReplacesTheCurrent()
        {
            Play(layer.Open(first));
            Play(layer.Open(second));
            Play(layer.Close());

            Assert.That(view.Log, Is.EqualTo(new[]
            {
                "show A", "dim on", "pop A open",
                "hide A", "show B", "pop B open",
                "dim off", "pop B close", "hide B"
            }));
            Assert.That(layer.IsOpen, Is.False);
        }

        [Test]
        public void ModalLayer_IgnoresOpeningTheShownModal_AndClosingAnEmptySlot()
        {
            Play(layer.Close());
            Play(layer.Open(first));
            Play(layer.Open(first));

            Assert.That(view.Log, Is.EqualTo(new[] { "show A", "dim on", "pop A open" }));
            Assert.That(layer.IsOpen, Is.True);
        }

        private static void Play(IStep step) => step.Play(CancellationToken.None).GetAwaiter().GetResult();
    }
}
