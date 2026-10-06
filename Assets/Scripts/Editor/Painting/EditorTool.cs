namespace Game.LevelEditor
{
    /// <summary>What a stroke does: a left-drag paints, a right-drag erases. Ctrl/Cmd + click selects (D59).</summary>
    public enum EditorTool
    {
        /// <summary>Paint cells with the brush.</summary>
        Paint,

        /// <summary>Clear cells.</summary>
        Erase
    }
}
