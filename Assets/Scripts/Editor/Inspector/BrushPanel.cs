using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>The controls (read-only: the mouse buttons and keys do the work), and the brush a left-drag lays down.</summary>
    internal sealed class BrushPanel
    {
        private static readonly string[] KindNames = { "Wall", "Door", "Block" };

        /// <summary>Input → what it does, one row each.</summary>
        private static readonly string[,] Controls =
        {
            { "Left-drag", "Paint" },
            { "Right-drag", "Erase" },
            { "Ctrl/Cmd + click", "Select" },
            { "Esc", "Deselect" },
            { "Del", "Delete selected" },
            { "Ctrl/Cmd + Z", "Undo" },
            { "Ctrl/Cmd + Shift + Z", "Redo" }
        };

        private readonly LevelEditorSession session;

        public BrushPanel(LevelEditorSession session) => this.session = session;

        public void Draw()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

            for (int row = 0; row < Controls.GetLength(0); row++)
            {
                EditorGUILayout.LabelField(Controls[row, 0], Controls[row, 1], EditorStyles.miniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            DrawBrush(session.Brush);
        }

        private void DrawBrush(Brush brush)
        {
            EditorGUI.BeginChangeCheck();
            var kind = (EntityKind)GUILayout.Toolbar((int)brush.Kind, KindNames);
            int color = kind == EntityKind.Wall ? brush.ColorId : ColorSwatches.Draw("Color", brush.ColorId);
            Direction direction = brush.Direction;

            if (kind == EntityKind.Door)
            {
                direction = (Direction)EditorGUILayout.EnumPopup("Inner direction", brush.Direction);
                EditorGUILayout.LabelField("Doors started on the edge point outward.", EditorStyles.miniLabel);
            }

            if (EditorGUI.EndChangeCheck())
                session.ChooseBrush(new Brush(kind, color, direction));
        }
    }
}
