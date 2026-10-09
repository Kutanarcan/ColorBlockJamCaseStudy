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
            Play(layer.Close(resume: true));

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
            Play(layer.Close(resume: true));
            Play(layer.Open(first));
            Play(layer.Open(first));

            Assert.That(view.Log, Is.EqualTo(new[] { "show A", "dim on", "pop A open" }));
            Assert.That(layer.IsOpen, Is.True);
        }

        [Test]
        public void ModalLayer_PausesPlayWhileShown_AndResumesOnlyWhenAsked()
        {
            layer.PauseWhileShown(new FakePausable(view.Log));

            Play(layer.Open(first));
            Play(layer.Open(second));
            Play(layer.Close(resume: false));
            Assert.That(view.Log, Has.None.EqualTo("resume"), "A close that restarts or leaves does not resume.");

            Play(layer.Open(first));
            Play(layer.Close(resume: true));
            Assert.That(view.Log[0], Is.EqualTo("pause"), "Play stops before the modal shows.");
            Assert.That(view.Log[view.Log.Count - 1], Is.EqualTo("resume"), "Play restarts once the modal is gone.");
        }

        private static void Play(IStep step) => step.Play(CancellationToken.None).GetAwaiter().GetResult();
    }
}
