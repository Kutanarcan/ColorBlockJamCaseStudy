using System.Collections.Generic;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// The Level Editor window: composition root and layout only. All behaviour lives in LevelEditorSession.
    /// The open level survives a domain reload as a serialized JSON snapshot (the undo history does not). Closing
    /// the window or Unity with unsaved changes asks to save them.
    /// </summary>
    public sealed class LevelEditorWindow : EditorWindow
    {
        [SerializeField] private string snapshot = "";
        [SerializeField] private string snapshotKey = "";
        [SerializeField] private string snapshotSavedKey = "";
        [SerializeField] private bool snapshotDirty;

        private LevelEditorSession session;
        private FileBar fileBar;
        private GridView grid;
        private InspectorView inspector;
        private Vector2 scroll;
        private bool dropFocus;

        [MenuItem("Tools/Color Block Jam/Level Editor")]
        public static void ShowWindow() => GetWindow<LevelEditorWindow>("Level Editor");

        public override void SaveChanges()
        {
            if (fileBar.Save())
                base.SaveChanges();
        }

        /// <summary>Called by <see cref="LevelEditorShortcuts"/>.</summary>
        internal void UndoEdit() => AfterShortcut(session.Undo);

        internal void RedoEdit() => AfterShortcut(session.Redo);

        internal void DeleteSelected() => AfterShortcut(session.DeleteSelected);


        private void OnEnable()
        {
            saveChangesMessage = "The level has unsaved changes. Save them?";
            session = new LevelEditorSession(ModifierCatalog.Default(), LevelRules.Discover());

            if (snapshot.Length == 0 || session.Restore(snapshotKey, snapshotSavedKey, snapshot, snapshotDirty).IsFailure)
                session.New(9, 11, 60f);

            fileBar = new FileBar(session);
            grid = new GridView(session);
            inspector = new InspectorView(session);
        }

        /// <summary>A mouse up released over another window never comes here; the stroke still closes and the rules run.</summary>
        private void OnLostFocus()
        {
            session.EndStroke();
            Repaint();
        }

        private void OnDisable()
        {
            snapshot = session.Document.Snapshot();
            snapshotKey = session.Document.Key;
            snapshotSavedKey = session.Document.SavedKey;
            snapshotDirty = session.Document.IsDirty;
        }

        private void OnGUI()
        {
            if (dropFocus)
            {
                GUI.FocusControl(null);
                dropFocus = false;
            }

            HandleEscape();
            fileBar.Draw();

            // Panels on the left at a fixed width; the grid takes the rest, so it grows to the right with the window.
            EditorGUILayout.BeginHorizontal();
            inspector.Draw();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            bool edited = grid.Draw();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            DrawViolations();
            hasUnsavedChanges = session.Document.IsDirty;

            if (edited)
                Repaint();
        }

        /// <summary>Shortcuts run outside OnGUI: drop keyboard focus on the next OnGUI so fields show restored values.</summary>
        private void AfterShortcut(System.Action action)
        {
            action();
            dropFocus = true;
            Repaint();
        }

        /// <summary>
        /// Esc deselects. Read from the key event, not the Shortcut Manager: Esc never reached a window shortcut.
        /// While a text field is edited, Esc stays with the field (it cancels the edit).
        /// </summary>
        private void HandleEscape()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown || e.keyCode != KeyCode.Escape || EditorGUIUtility.editingTextField)
                return;

            session.Deselect();
            e.Use();
            Repaint();
        }

        private void DrawViolations()
        {
            IReadOnlyList<RuleViolation> violations = session.Document.Violations;
            bool pass = violations.Count == 0;
            EditorTheme.BeginSection(pass ? "Rules · all pass" : $"Rules · {violations.Count} broken",
                pass ? EditorTheme.Success : EditorTheme.Warning);

            foreach (RuleViolation violation in violations)
            {
                EditorGUILayout.BeginHorizontal();
                EditorTheme.Chip(EditorTheme.Warning, 6f);
                GUILayout.Label(violation.Message, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorTheme.EndSection();
        }
    }
}
