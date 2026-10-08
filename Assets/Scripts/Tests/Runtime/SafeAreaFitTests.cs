using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class SafeAreaFitTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void SafeArea_FitsTheRect_ForA20x9Notch()
        {
            // 1080×2400 (20:9): a 110 px notch on top, a 66 px gesture bar at the bottom.
            Rect anchors = SafeAreaFit.Anchors(new Vector2(1080f, 2400f), new Rect(0f, 66f, 1080f, 2400f - 66f - 110f));

            AssertAnchors(anchors, 0f, 66f / 2400f, 1f, 2290f / 2400f);
        }

        [Test]
        public void NoNotch_KeepsTheWholeScreen()
        {
            Rect anchors = SafeAreaFit.Anchors(new Vector2(1080f, 1920f), new Rect(0f, 0f, 1080f, 1920f));

            AssertAnchors(anchors, 0f, 0f, 1f, 1f);
        }

        [Test]
        public void SideInsets_MoveTheLeftAndRightEdges()
        {
            Rect anchors = SafeAreaFit.Anchors(new Vector2(2400f, 1080f), new Rect(100f, 0f, 2200f, 1080f));

            AssertAnchors(anchors, 100f / 2400f, 0f, 2300f / 2400f, 1f);
        }

        [Test]
        public void SafeAreaPastTheScreen_IsCutToIt()
        {
            Rect anchors = SafeAreaFit.Anchors(new Vector2(1000f, 2000f), new Rect(-50f, 100f, 1100f, 2000f));

            AssertAnchors(anchors, 0f, 0.05f, 1f, 1f);
        }

        [TestCase(0f, 0f, 0f, 0f, 0f, 0f)]
        [TestCase(1080f, 1920f, 0f, 0f, 0f, 0f)]
        public void EmptyScreenOrSafeArea_KeepsTheWholeScreen(float width, float height, float x, float y, float w,
            float h)
        {
            Rect anchors = SafeAreaFit.Anchors(new Vector2(width, height), new Rect(x, y, w, h));

            AssertAnchors(anchors, 0f, 0f, 1f, 1f);
        }

        private static void AssertAnchors(Rect anchors, float xMin, float yMin, float xMax, float yMax)
        {
            Assert.That(anchors.xMin, Is.EqualTo(xMin).Within(Tolerance));
            Assert.That(anchors.yMin, Is.EqualTo(yMin).Within(Tolerance));
            Assert.That(anchors.xMax, Is.EqualTo(xMax).Within(Tolerance));
            Assert.That(anchors.yMax, Is.EqualTo(yMax).Within(Tolerance));
        }
    }
}
