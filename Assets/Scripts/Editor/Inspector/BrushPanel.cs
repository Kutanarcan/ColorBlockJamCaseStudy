using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>The brush a left-drag lays down (D53, D57).</summary>
    internal sealed class BrushPanel
    {
        private static readonly string[] KindNames = { "Wall", "Door", "Block" };

        private readonly LevelEditorSession session;

        public BrushPanel(LevelEditorSession session) => this.session = session;

        public void Draw()
        {
            EditorTheme.BeginSection("Brush");
            DrawBrush(session.Brush);
            EditorTheme.EndSection();
        }

        private void DrawBrush(Brush brush)
        {
            EditorGUI.BeginChangeCheck();
            EntityKind kind = DrawKinds(brush.Kind);
            int color = kind == EntityKind.Wall ? brush.ColorId : ColorSwatches.Draw("Color", brush.ColorId);
            Direction direction = brush.Direction;

            if (kind == EntityKind.Door)
            {
                direction = (Direction)EditorGUILayout.EnumPopup("Inner direction", brush.Direction);
                GUILayout.Label("Doors started on the edge point outward.", EditorTheme.Note);
            }

            if (EditorGUI.EndChangeCheck())
                session.ChooseBrush(new Brush(kind, color, direction));
        }

        /// <summary>One button per kind; the chosen one is tinted, the others plain.</summary>
        private static EntityKind DrawKinds(EntityKind chosen)
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < KindNames.Length; i++)
            {
                var kind = (EntityKind)i;
                GUIStyle style = i == 0 ? EditorStyles.miniButtonLeft
                    : i == KindNames.Length - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;

                using (kind == chosen ? ButtonTint.Primary() : ButtonTint.None())
                {
                    if (GUILayout.Toggle(kind == chosen, KindNames[i], style, GUILayout.Height(24f)) && kind != chosen)
                    {
                        GUI.changed = true;
                        chosen = kind;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            return chosen;
        }
    }
}
