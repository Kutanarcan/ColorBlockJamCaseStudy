namespace Game.Infrastructure
{
    /// <summary>
    /// Addressables labels. Labels are used only to download, never to load in game code (D65, D115).
    /// </summary>
    public static class ContentLabels
    {
        /// <summary>Everything that would come from a server: what the content update downloads.</summary>
        public const string Remote = "remote";
    }
}
