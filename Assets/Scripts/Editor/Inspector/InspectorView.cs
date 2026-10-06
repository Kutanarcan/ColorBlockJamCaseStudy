using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Side panel: the level, brush, selection and entity list panels in one scroll view.</summary>
    internal sealed class InspectorView
    {
        private const float Width = 300f;

        private readonly LevelPanel level;
        private readonly BrushPanel brush;
        private readonly SelectionPanel selection;
        private readonly EntityListPanel entities;
        private Vector2 scroll;

        public InspectorView(LevelEditorSession session)
        {
            level = new LevelPanel(session);
            brush = new BrushPanel(session);
            selection = new SelectionPanel(session);
            entities = new EntityListPanel(session);
        }

        public void Draw()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(Width));
            level.Draw();
            brush.Draw();
            selection.Draw();
            entities.Draw();
            EditorGUILayout.EndScrollView();
        }
    }
}
