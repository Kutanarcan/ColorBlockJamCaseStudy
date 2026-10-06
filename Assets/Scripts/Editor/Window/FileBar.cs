using System.IO;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Toolbar, left to right: open, save, new level (size, time, New), undo / redo; and the last file message.</summary>
    internal sealed class FileBar
    {
        private readonly LevelEditorSession session;
        private int newWidth = 9;
        private int newHeight = 11;
        private float newTime = 60f;
        private string message = "";

        public FileBar(LevelEditorSession session) => this.session = session;

        private LevelDocument Document => session.Document;

        public void Draw()
        {
            Rect bar = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawOpen();

            using (Document.IsDirty ? ButtonTint.Primary() : ButtonTint.None())
            {
                if (GUILayout.Button(Document.IsDirty ? "Save *" : "Save", EditorStyles.toolbarButton)) Save();
            }

            DrawBarColorAfterLastControl(bar);

            GUILayout.Space(8f);
            DrawNewSize();
            DrawNewButton();
            GUILayout.Space(8f);
            DrawHistory();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (message.Length > 0)
                EditorGUILayout.HelpBox(message, MessageType.None);
        }

        /// <summary>
        /// Writes the level. A level opened or saved before goes back to its own file; a new one is named in Unity's
        /// save dialog first. Rules are checked before asking for a name. True when saved.
        /// </summary>
        public bool Save()
        {
            Result rules = Document.CheckRules();

            if (rules.IsFailure)
            {
                message = rules.Error;

                return false;
            }

            if (Document.SavedKey.Length > 0)
            {
                Document.Key = Document.SavedKey;
            }
            else if (LevelFiles.TryAskKey(out string key, out string error))
            {
                Document.Key = key;
            }
            else
            {
                message = error;

                return false;
            }

            Result<string> json = Document.Save();

            if (json.IsFailure)
            {
                message = json.Error;

                return false;
            }

            LevelFiles.Write(Document.Key, json.Value);
            Document.MarkSaved();
            message = $"Saved {LevelFiles.PathOf(Document.Key)}";

            return true;
        }

        /// <summary>Colors the bar from the last drawn control (Save) to its end; Open and Save keep the plain toolbar.</summary>
        private static void DrawBarColorAfterLastControl(Rect bar)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float start = GUILayoutUtility.GetLastRect().xMax;
            EditorGUI.DrawRect(new Rect(start, bar.y, bar.xMax - start, bar.height), EditorTheme.BarBackground);
        }

        private void DrawNewButton()
        {
            using (ButtonTint.Primary())
            {
                if (GUILayout.Button("New", EditorStyles.toolbarButton) && ConfirmDiscard())
                {
                    session.New(newWidth, newHeight, newTime);
                    message = "";
                }
            }
        }

        /// <summary>The size and time a new level gets, with accent labels.</summary>
        private void DrawNewSize()
        {
            GUILayout.Label("W", EditorTheme.ToolbarLabel, GUILayout.Width(14f));
            newWidth = Clamp(EditorGUILayout.IntField(newWidth, GUILayout.Width(30f)));
            GUILayout.Label("H", EditorTheme.ToolbarLabel, GUILayout.Width(14f));
            newHeight = Clamp(EditorGUILayout.IntField(newHeight, GUILayout.Width(30f)));
            GUILayout.Label("Time", EditorTheme.ToolbarLabel, GUILayout.Width(32f));
            newTime = Mathf.Max(1f, EditorGUILayout.FloatField(newTime, GUILayout.Width(40f)));
        }

        /// <summary>Undo / redo drop keyboard focus, so a focused field shows the restored value.</summary>
        private void DrawHistory()
        {
            using (new EditorGUI.DisabledScope(!Document.CanUndo))
            {
                if (GUILayout.Button("Undo", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    session.Undo();
                }
            }

            using (new EditorGUI.DisabledScope(!Document.CanRedo))
            {
                if (GUILayout.Button("Redo", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    session.Redo();
                }
            }
        }

        private void DrawOpen()
        {
            if (!GUILayout.Button("Open ▾", EditorStyles.toolbarDropDown))
                return;

            var menu = new GenericMenu();
            string[] keys = LevelFiles.Keys();

            if (keys.Length == 0)
                menu.AddDisabledItem(new GUIContent($"No levels in {LevelFiles.Folder}"));

            foreach (string key in keys)
            {
                menu.AddItem(new GUIContent(key), key == Document.Key, () => Open(key));
            }

            menu.ShowAsContext();
        }

        private void Open(string key)
        {
            if (!ConfirmDiscard())
                return;

            string text;

            try
            {
                text = LevelFiles.Read(key);
            }
            catch (IOException e)
            {
                message = $"Cannot read {LevelFiles.PathOf(key)}: {e.Message}";

                return;
            }

            Result opened = session.Open(key, text);
            message = opened.IsSuccess ? $"Opened {LevelFiles.PathOf(key)}" : $"Cannot open {key}: {opened.Error}";
        }

        private bool ConfirmDiscard() =>
            !Document.IsDirty || EditorUtility.DisplayDialog("Unsaved changes",
                "The current level has unsaved changes. Discard them?", "Discard", "Cancel");

        private static int Clamp(int size) => Mathf.Clamp(size, LevelTemplates.MinSize, LevelTemplates.MaxSize);
    }
}
