namespace Game.LevelEditor
{
    /// <summary>The controls, read-only: the mouse buttons and keys do the work (D59). Collapsible.</summary>
    internal static class ControlsPanel
    {
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

        public static void Draw()
        {
            if (EditorTheme.BeginFoldoutSection("Controls"))
            {
                for (int row = 0; row < Controls.GetLength(0); row++)
                {
                    EditorTheme.KeyRow(Controls[row, 0], Controls[row, 1]);
                }
            }

            EditorTheme.EndSection();
        }
    }
}
