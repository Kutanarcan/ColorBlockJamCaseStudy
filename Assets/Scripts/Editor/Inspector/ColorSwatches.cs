using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>A color id picker: one clickable swatch per preview color, and a number field for ids beyond them.</summary>
    internal static class ColorSwatches
    {
        private const float Size = 18f;
        private const float Ring = 2f;

        public static int Draw(string label, int colorId)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(40f));

            for (int id = 0; id < PreviewColors.Count; id++)
            {
                Rect rect = GUILayoutUtility.GetRect(Size, Size, GUILayout.Width(Size), GUILayout.Height(Size));
                DrawSwatch(rect, id, id == colorId);

                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    colorId = id;
            }

            colorId = Mathf.Max(0, EditorGUILayout.IntField(colorId, GUILayout.Width(32f)));
            EditorGUILayout.EndHorizontal();

            return colorId;
        }

        private static void DrawSwatch(Rect rect, int colorId, bool chosen)
        {
            if (chosen)
            {
                EditorGUI.DrawRect(rect, PreviewColors.Selection);
                rect = new Rect(rect.x + Ring, rect.y + Ring, rect.width - 2f * Ring, rect.height - 2f * Ring);
            }

            EditorGUI.DrawRect(rect, PreviewColors.Of(colorId));
        }
    }
}
