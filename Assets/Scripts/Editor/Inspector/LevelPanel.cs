using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Level settings: the time limit, and the size with a shrink / grow pair per side (D54).</summary>
    internal sealed class LevelPanel
    {
        private static readonly Direction[] Sides = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        private static readonly string[] SideNames = { "Top", "Bottom", "Left", "Right" };

        private readonly LevelEditorSession session;

        public LevelPanel(LevelEditorSession session) => this.session = session;

        public void Draw()
        {
            if (EditorTheme.BeginFoldoutSection("Level"))
                DrawContent(session.Model);

            EditorTheme.EndSection();
        }

        private void DrawContent(LevelModel model)
        {
            float time = EditorGUILayout.FloatField("Time limit (s)", model.TimeLimit);

            if (!Mathf.Approximately(time, model.TimeLimit))
                session.SetTimeLimit(time);

            EditorGUILayout.LabelField("Size", $"{model.Width} x {model.Height}");

            for (int i = 0; i < Sides.Length; i++)
            {
                DrawSide(Sides[i], SideNames[i]);
            }
        }

        private void DrawSide(Direction side, string name)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(EditorGUIUtility.labelWidth));

            using (ButtonTint.Danger())
            using (new EditorGUI.DisabledScope(!LevelResizer.CanResize(session.Model, side, -1)))
            {
                if (GUILayout.Button("-", EditorStyles.miniButtonLeft))
                    Shrink(side, name);
            }

            using (ButtonTint.Positive())
            using (new EditorGUI.DisabledScope(!LevelResizer.CanResize(session.Model, side, 1)))
            {
                if (GUILayout.Button("+", EditorStyles.miniButtonRight))
                    session.Resize(side, 1);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Shrink(Direction side, string name)
        {
            int cut = LevelResizer.CutCount(session.Model, side);

            if (cut > 0 && !EditorUtility.DisplayDialog("Shrink level",
                    $"Shrinking the {name.ToLowerInvariant()} side removes {cut} occupied cell(s). Undo brings them back.",
                    "Shrink", "Cancel"))
                return;

            session.Resize(side, -1);
        }
    }
}
