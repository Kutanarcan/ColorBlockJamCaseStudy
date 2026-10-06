using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Places a perspective camera so a board box fills a viewport rect (D73). Rotation and field of view stay as
    /// set in the scene; only the position changes: a distance back along the view and a shift across it.
    /// Corner i at camera depth z_i fits on one axis when lo·z_i ≤ s_i − shift ≤ hi·z_i. Some shift fits every
    /// corner when each lower bound is under each upper bound; solving that pair-wise for the distance gives the
    /// smallest distance, and the shift is centred in what is left. The tighter axis touches both of its edges.
    /// </summary>
    public static class CameraFit
    {
        private const int Corners = 8;

        /// <summary>The safe area minus the margins, as a viewport rect (0..1).</summary>
        public static Rect Viewport(Vector2 screen, Rect safeArea, FitMargins margins)
        {
            float xMin = safeArea.xMin / screen.x + margins.Side;
            float xMax = safeArea.xMax / screen.x - margins.Side;
            float yMin = safeArea.yMin / screen.y + margins.Bottom;
            float yMax = safeArea.yMax / screen.y - margins.Top;

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        /// <summary>Assumes a non-empty viewport and a camera that looks down at the board.</summary>
        public static Vector3 Position(Bounds board, Quaternion rotation, float verticalFov, float aspect, Rect viewport)
        {
            float tanV = Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad);
            float tanH = tanV * aspect;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            float loX = Ndc(viewport.xMin) * tanH, hiX = Ndc(viewport.xMax) * tanH;
            float loY = Ndc(viewport.yMin) * tanV, hiY = Ndc(viewport.yMax) * tanV;

            float distance = Mathf.Max(
                Distance(board, right, forward, loX, hiX),
                Distance(board, up, forward, loY, hiY));

            return board.center
                   - forward * distance
                   + right * Shift(board, right, forward, distance, loX, hiX)
                   + up * Shift(board, up, forward, distance, loY, hiY);
        }

        /// <summary>The smallest distance at which some shift along <paramref name="axis"/> fits every corner.</summary>
        private static float Distance(Bounds board, Vector3 axis, Vector3 forward, float lo, float hi)
        {
            float distance = float.MinValue;

            for (int i = 0; i < Corners; i++)
            {
                Vector3 a = Corner(board, i) - board.center;

                for (int j = 0; j < Corners; j++)
                {
                    Vector3 b = Corner(board, j) - board.center;
                    float needed = (Vector3.Dot(a, axis) - Vector3.Dot(b, axis)
                                    - hi * Vector3.Dot(a, forward) + lo * Vector3.Dot(b, forward)) / (hi - lo);
                    distance = Mathf.Max(distance, needed);
                }
            }

            return distance;
        }

        /// <summary>The middle of the shifts that fit every corner at <paramref name="distance"/>.</summary>
        private static float Shift(Bounds board, Vector3 axis, Vector3 forward, float distance, float lo, float hi)
        {
            float min = float.MinValue;
            float max = float.MaxValue;

            for (int i = 0; i < Corners; i++)
            {
                Vector3 corner = Corner(board, i) - board.center;
                float across = Vector3.Dot(corner, axis);
                float depth = Vector3.Dot(corner, forward) + distance;
                min = Mathf.Max(min, across - hi * depth);
                max = Mathf.Min(max, across - lo * depth);
            }

            return (min + max) * 0.5f;
        }

        private static Vector3 Corner(Bounds box, int index) => new Vector3(
            (index & 1) == 0 ? box.min.x : box.max.x,
            (index & 2) == 0 ? box.min.y : box.max.y,
            (index & 4) == 0 ? box.min.z : box.max.z);

        private static float Ndc(float viewport) => viewport * 2f - 1f;
    }
}
