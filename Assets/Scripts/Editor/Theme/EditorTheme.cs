using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// The Level Editor's look in one place: colors for both editor skins, spacing, section cards and the controls table. Styles are
    /// built on first use, inside OnGUI, where Unity's skin is available. Colors are drawn with DrawRect, so no
    /// textures are created.
    /// </summary>
    internal static class EditorTheme
    {
        public const float InspectorWidth = 320f;
        public const float GridPadding = 16f;
        private const float AccentWidth = 3f;
        private const float KeyWidth = 150f;
        private const float RowHeight = 20f;
        private const float ArrowWidth = 20f;

        private static GUIStyle card;
        private static GUIStyle header;
        private static GUIStyle note;
        private static GUIStyle toolbarLabel;
        private static GUIStyle arrow;

        private static bool Dark => EditorGUIUtility.isProSkin;

        public static Color Accent => new Color(0.30f, 0.62f, 1f);
        public static Color Warning => new Color(1f, 0.62f, 0.20f);
        public static Color Success => new Color(0.35f, 0.78f, 0.45f);
        public static Color CardBackground => Dark ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.86f, 0.86f, 0.86f);
        public static Color CardBorder => Dark ? new Color(0.36f, 0.36f, 0.36f) : new Color(0.66f, 0.66f, 0.66f);
        public static Color HeaderBackground => Dark ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.76f, 0.76f, 0.76f);

        /// <summary>The top toolbar: a blue-gray, so the bar reads apart from the panels below.</summary>
        public static Color BarBackground => Dark ? new Color(0.17f, 0.24f, 0.34f) : new Color(0.62f, 0.72f, 0.86f);

        private static GUIStyle Card => card ?? (card = new GUIStyle
        {
            padding = new RectOffset(10, 8, 6, 8),
            margin = new RectOffset(4, 4, 4, 4)
        });

        private static GUIStyle Header => header ?? (header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 0, 2, 10)
        });

        /// <summary>A bold, accent-colored label for toolbar fields.</summary>
        public static GUIStyle ToolbarLabel => toolbarLabel ?? (toolbarLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = Accent }
        });

        private static GUIStyle Arrow => arrow ?? (arrow = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10
        });

        public static GUIStyle Note => note ?? (note = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            normal = { textColor = Dark ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.35f, 0.35f, 0.35f) }
        });

        /// <summary>Starts a card with a title. Always pair with <see cref="EndSection"/>.</summary>
        public static void BeginSection(string title) => BeginCard(title, out _);

        /// <summary>
        /// A card whose header has an up / down arrow on its left that collapses or expands it; the state is remembered
        /// per title in EditorPrefs. Returns the new state: draw the content only when expanded, and always call
        /// <see cref="EndSection"/>.
        /// </summary>
        public static bool BeginFoldoutSection(string title)
        {
            string key = "ColorBlockJam.LevelEditor.Expanded." + title;
            bool expanded = EditorPrefs.GetBool(key, true);
            bool now = BeginFoldoutSection(title, expanded);

            if (now != expanded)
                EditorPrefs.SetBool(key, now);

            return now;
        }

        /// <summary>Same, with the state kept by the caller instead of EditorPrefs.</summary>
        public static bool BeginFoldoutSection(string title, bool expanded)
        {
            BeginCard(title, out Rect header);
            var arrow = new Rect(header.x, header.y, ArrowWidth, header.height);

            if (GUI.Button(arrow, expanded ? "▲" : "▼", Arrow))
                expanded = !expanded;

            return expanded;
        }

        /// <summary>A card with a colored stripe on its left, for a status (the rules).</summary>
        public static void BeginSection(string title, Color stripe)
        {
            Rect rect = BeginCard(title, out _);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, AccentWidth, rect.height), stripe);
        }

        private static Rect BeginCard(string title, out Rect header)
        {
            Rect rect = EditorGUILayout.BeginVertical(Card);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, CardBackground);
                DrawBorder(rect, CardBorder);
            }

            header = DrawHeader(rect, title);

            return rect;
        }

        public static void EndSection() => EditorGUILayout.EndVertical();

        /// <summary>The title on a band across the card's full width, from its top border to just below the text.</summary>
        private static Rect DrawHeader(Rect card, string title)
        {
            Rect text = GUILayoutUtility.GetRect(new GUIContent(title), Header);

            if (Event.current.type == EventType.Repaint)
            {
                float top = card.y + 1f;
                EditorGUI.DrawRect(new Rect(card.x + 1f, top, card.width - 2f, text.yMax + 4f - top), HeaderBackground);
            }

            GUI.Label(text, title, Header);

            return text;
        }

        /// <summary>A 1px line on each side of <paramref name="rect"/>.</summary>
        private static void DrawBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }

        /// <summary>One "input → effect" row of a two-column table: the input in bold on the left, the effect beside it.</summary>
        public static void KeyRow(string input, string effect)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(RowHeight));
            GUILayout.Label(input, EditorStyles.boldLabel, GUILayout.Width(KeyWidth), GUILayout.Height(RowHeight));
            GUILayout.Label(effect, EditorStyles.label, GUILayout.Height(RowHeight));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>A small filled square, e.g. an entity's color in a list.</summary>
        public static void Chip(Color color, float size = 12f)
        {
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(16f));
            rect.y += (rect.height - size) * 0.5f;
            rect.height = size;

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, color);
        }
    }
}
