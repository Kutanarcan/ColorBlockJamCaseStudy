using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class CameraFitTests
    {
        private const float Fov = 34.1f;
        private const float Tolerance = 1e-3f;

        // The prototype camera: 74° down, no yaw.
        private static readonly Quaternion Pitch = Quaternion.Euler(74f, 0f, 0f);
        private static readonly FitMargins Margins = new FitMargins(0.15f, 0.12f, 0.04f);

        // Square, tall and wide boards, in cells.
        private static readonly Vector2Int[] Boards = { new Vector2Int(7, 7), new Vector2Int(6, 11), new Vector2Int(11, 6) };

        [TestCase(1080, 1920)]
        [TestCase(1080, 2160)]
        [TestCase(1080, 2340)]
        [TestCase(1080, 2400)]
        public void CameraFit_KeepsTheBoardInside_From16x9To20x9(int width, int height)
        {
            var screen = new Vector2(width, height);
            var safeArea = new Rect(0f, 60f, width, height - 160f); // notch on top, gesture bar at the bottom
            Rect viewport = CameraFit.Viewport(screen, safeArea, Margins);
            float aspect = screen.x / screen.y;

            foreach (Vector2Int cells in Boards)
            {
                var size = new Vector3(cells.x * 2f, 1f, cells.y * 2f);
                var board = new Bounds(size * 0.5f, size);
                Vector3 position = CameraFit.Position(board, Pitch, Fov, aspect, viewport);

                AssertFitsTightly(board, position, aspect, viewport, $"{cells.x}x{cells.y} on {width}x{height}");
            }
        }

        [Test]
        public void Viewport_LeavesTheSafeAreaAndTheMarginsFree()
        {
            Rect viewport = CameraFit.Viewport(new Vector2(1000f, 2000f), new Rect(0f, 100f, 1000f, 1800f), Margins);

            Assert.That(viewport.xMin, Is.EqualTo(0.04f).Within(Tolerance));
            Assert.That(viewport.xMax, Is.EqualTo(0.96f).Within(Tolerance));
            Assert.That(viewport.yMin, Is.EqualTo(0.05f + 0.12f).Within(Tolerance));
            Assert.That(viewport.yMax, Is.EqualTo(0.95f - 0.15f).Within(Tolerance));
        }

        /// <summary>Every corner inside the viewport, and the tighter axis touching both of its edges.</summary>
        private static void AssertFitsTightly(Bounds board, Vector3 position, float aspect, Rect viewport, string label)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            foreach (float x in new[] { board.min.x, board.max.x })
            foreach (float y in new[] { board.min.y, board.max.y })
            foreach (float z in new[] { board.min.z, board.max.z })
            {
                Vector2 point = ToViewport(new Vector3(x, y, z), position, aspect);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            Assert.That(min.x, Is.GreaterThanOrEqualTo(viewport.xMin - Tolerance), label + ", left");
            Assert.That(max.x, Is.LessThanOrEqualTo(viewport.xMax + Tolerance), label + ", right");
            Assert.That(min.y, Is.GreaterThanOrEqualTo(viewport.yMin - Tolerance), label + ", bottom");
            Assert.That(max.y, Is.LessThanOrEqualTo(viewport.yMax + Tolerance), label + ", top");

            bool fillsWidth = Mathf.Abs(min.x - viewport.xMin) < Tolerance && Mathf.Abs(max.x - viewport.xMax) < Tolerance;
            bool fillsHeight = Mathf.Abs(min.y - viewport.yMin) < Tolerance && Mathf.Abs(max.y - viewport.yMax) < Tolerance;
            Assert.That(fillsWidth || fillsHeight, label + ", not as close as it could be");
        }

        /// <summary>Unity's own camera matrices, so the test does not reuse the fit's math.</summary>
        private static Vector2 ToViewport(Vector3 world, Vector3 cameraPosition, float aspect)
        {
            Matrix4x4 worldToCamera = Matrix4x4.TRS(cameraPosition, Pitch, new Vector3(1f, 1f, -1f)).inverse;
            Vector4 clip = Matrix4x4.Perspective(Fov, aspect, 0.3f, 1000f) * worldToCamera
                           * new Vector4(world.x, world.y, world.z, 1f);

            Assert.That(clip.w, Is.GreaterThan(0f), "corner behind the camera");

            return new Vector2(clip.x / clip.w * 0.5f + 0.5f, clip.y / clip.w * 0.5f + 0.5f);
        }
    }
}
