using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// Temporary colors for colorIds until the palette exists (presentation phase). Editor display only:
    /// the level stores colorIds, never colors.
    /// </summary>
    internal static class PreviewColors
    {
        private static readonly Color[] Colors =
        {
            new Color(0.90f, 0.25f, 0.25f), new Color(0.25f, 0.55f, 0.95f), new Color(0.30f, 0.80f, 0.35f),
            new Color(0.95f, 0.80f, 0.20f), new Color(0.65f, 0.35f, 0.85f), new Color(0.95f, 0.55f, 0.20f),
            new Color(0.25f, 0.80f, 0.80f), new Color(0.95f, 0.45f, 0.70f)
        };

        public static readonly Color Empty = new Color(0.18f, 0.18f, 0.18f);
        public static readonly Color Wall = new Color(0.40f, 0.40f, 0.40f);
        public static readonly Color Selection = Color.white;
        public static readonly Color Violation = new Color(1f, 0.15f, 0.15f);

        /// <summary>Distinct preview colors; ids from this count on repeat them.</summary>
        public static int Count => Colors.Length;

        public static Color Of(int colorId) => Colors[((colorId % Colors.Length) + Colors.Length) % Colors.Length];
    }
}
