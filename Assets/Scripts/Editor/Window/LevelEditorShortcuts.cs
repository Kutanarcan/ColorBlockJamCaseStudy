using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// Level Editor keys, active while its window has focus and rebindable under Edit → Shortcuts. Bound to the
    /// window, they take priority there over Unity's global Undo / Redo. Single keys do nothing while a text
    /// field is being edited.
    /// </summary>
    internal static class LevelEditorShortcuts
    {
        private const string Prefix = "Color Block Jam/Level Editor/";

        [Shortcut(Prefix + "Undo", typeof(LevelEditorWindow), KeyCode.Z, ShortcutModifiers.Action)]
        private static void Undo(ShortcutArguments args) => WindowOf(args).UndoEdit();

        [Shortcut(Prefix + "Redo", typeof(LevelEditorWindow), KeyCode.Z, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void Redo(ShortcutArguments args) => WindowOf(args).RedoEdit();

        [Shortcut(Prefix + "Redo (Y)", typeof(LevelEditorWindow), KeyCode.Y, ShortcutModifiers.Action)]
        private static void RedoY(ShortcutArguments args) => WindowOf(args).RedoEdit();

        [Shortcut(Prefix + "Delete Selected", typeof(LevelEditorWindow), KeyCode.Delete)]
        private static void Delete(ShortcutArguments args)
        {
            if (!EditorGUIUtility.editingTextField)
                WindowOf(args).DeleteSelected();
        }

        private static LevelEditorWindow WindowOf(ShortcutArguments args) => (LevelEditorWindow)args.context;
    }
}
