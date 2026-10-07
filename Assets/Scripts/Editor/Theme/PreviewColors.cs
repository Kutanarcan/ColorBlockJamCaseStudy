using Game.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// The editor's colors for colorIds, from the game's palette asset (P9.3), so a level looks in the editor as it
    /// does in the game. Editor display only: the level stores colorIds, never colors. Without a palette asset the
    /// old fixed list stands in.
    /// </summary>
    internal static class PreviewColors
    {
        private static readonly Color[] Fallback =
        {
            new Color(0.90f, 0.25f, 0.25f), new Color(0.25f, 0.55f, 0.95f), new Color(0.30f, 0.80f, 0.35f),
            new Color(0.95f, 0.80f, 0.20f), new Color(0.65f, 0.35f, 0.85f), new Color(0.95f, 0.55f, 0.20f),
            new Color(0.25f, 0.80f, 0.80f), new Color(0.95f, 0.45f, 0.70f)
        };

        public static readonly Color Empty = new Color(0.18f, 0.18f, 0.18f);
        public static readonly Color Wall = new Color(0.40f, 0.40f, 0.40f);
        public static readonly Color Selection = Color.white;
        public static readonly Color Violation = new Color(1f, 0.15f, 0.15f);

        // The asset itself, found once per domain load; edits to it show at once.
        private static Palette palette;

        /// <summary>The colors a brush offers: the palette's, or the fallback list without one.</summary>
        public static int Count => HasPalette ? CurrentPalette.Count : Fallback.Length;

        /// <summary>How many colorIds the game can draw; with no palette asset, every id passes.</summary>
        public static int PaletteCount => HasPalette ? CurrentPalette.Count : int.MaxValue;

        public static Color Of(int colorId)
        {
            if (HasPalette)
                return colorId >= 0 && colorId < CurrentPalette.Count ? CurrentPalette.ColorOf(colorId) : Violation;

            return Fallback[((colorId % Fallback.Length) + Fallback.Length) % Fallback.Length];
        }

        private static bool HasPalette => CurrentPalette != null && CurrentPalette.Count > 0;

        private static Palette CurrentPalette
        {
            get
            {
                if (palette == null)
                    palette = FindPalette();

                return palette;
            }
        }

        private static Palette FindPalette()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(Palette));

            return guids.Length == 0
                ? null
                : AssetDatabase.LoadAssetAtPath<Palette>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
