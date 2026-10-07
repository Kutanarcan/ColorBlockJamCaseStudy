using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>The toolbar's undo / redo. Both drop keyboard focus, so a focused field shows the restored value.</summary>
    internal sealed class HistoryButtons
    {
        private readonly LevelEditorSession session;

        public HistoryButtons(LevelEditorSession session) => this.session = session;

        public void Draw()
        {
            using (new EditorGUI.DisabledScope(!session.Document.CanUndo))
            {
                if (GUILayout.Button("Undo", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    session.Undo();
                }
            }

            using (new EditorGUI.DisabledScope(!session.Document.CanRedo))
            {
                if (GUILayout.Button("Redo", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    session.Redo();
                }
            }
        }
    }
}
