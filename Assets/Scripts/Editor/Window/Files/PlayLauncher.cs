using Game.LevelTest;

namespace Game.LevelEditor
{
    /// <summary>
    /// Plays a saved level from the Level Editor (D75, D131): a level test, in its own scene and on its own services,
    /// so testing never touches the player's save. The level is already addressable, saved just before.
    /// </summary>
    internal static class PlayLauncher
    {
        public static void Play(string levelKey) => LevelTestLauncher.Play(levelKey);
    }
}
