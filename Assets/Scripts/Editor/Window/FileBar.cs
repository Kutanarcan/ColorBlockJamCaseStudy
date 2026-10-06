using System.IO;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Toolbar: new level, undo / redo, open, key, save / overwrite, and the last file message.</summary>
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
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawNew();
            DrawHistory();
            GUILayout.FlexibleSpace();
            DrawOpen();
            Document.Key = EditorGUILayout.TextField(Document.Key, EditorStyles.toolbarTextField, GUILayout.Width(160f));
            if (GUILayout.Button(Document.IsDirty ? "Save *" : "Save", EditorStyles.toolbarButton)) Save();
            EditorGUILayout.EndHorizontal();

            if (message.Length > 0)
                EditorGUILayout.HelpBox(message, MessageType.None);
        }

        /// <summary>Writes the level under its key; asks before overwriting another level's file. True when saved.</summary>
        public bool Save()
        {
            Result<string> json = Document.Save();

            if (json.IsFailure)
            {
                message = json.Error;

                return false;
            }

            if (LevelFiles.Exists(Document.Key) && Document.Key != Document.SavedKey &&
                !EditorUtility.DisplayDialog("Overwrite level",
                    $"{LevelFiles.PathOf(Document.Key)} already exists. Overwrite it?", "Overwrite", "Cancel"))
                return false;

            LevelFiles.Write(Document.Key, json.Value);
            Document.MarkSaved();
            message = $"Saved {LevelFiles.PathOf(Document.Key)}";

            return true;
        }

        private void DrawNew()
        {
            GUILayout.Label("W", GUILayout.Width(14f));
            newWidth = Clamp(EditorGUILayout.IntField(newWidth, GUILayout.Width(30f)));
            GUILayout.Label("H", GUILayout.Width(14f));
            newHeight = Clamp(EditorGUILayout.IntField(newHeight, GUILayout.Width(30f)));
            GUILayout.Label("Time", GUILayout.Width(32f));
            newTime = Mathf.Max(1f, EditorGUILayout.FloatField(newTime, GUILayout.Width(40f)));

            if (GUILayout.Button("New", EditorStyles.toolbarButton) && ConfirmDiscard())
            {
                session.New(newWidth, newHeight, newTime);
                message = "";
            }
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
