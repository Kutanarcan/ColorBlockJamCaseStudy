using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The screen's safe area as anchors (0..1) for a rect that fills the screen (D76). A safe area reaching past
    /// the screen is cut to it; an empty screen or safe area keeps the whole screen.
    /// </summary>
    public static class SafeAreaFit
    {
        public static Rect Anchors(Vector2 screen, Rect safeArea)
        {
            if (screen.x <= 0f || screen.y <= 0f)
                return Full;

            float xMin = Mathf.Clamp01(safeArea.xMin / screen.x);
            float xMax = Mathf.Clamp01(safeArea.xMax / screen.x);
            float yMin = Mathf.Clamp01(safeArea.yMin / screen.y);
            float yMax = Mathf.Clamp01(safeArea.yMax / screen.y);

            return xMax > xMin && yMax > yMin ? Rect.MinMaxRect(xMin, yMin, xMax, yMax) : Full;
        }

        private static Rect Full => Rect.MinMaxRect(0f, 0f, 1f, 1f);
    }
}
