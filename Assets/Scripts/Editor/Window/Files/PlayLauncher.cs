using Game.Infrastructure;
using UnityEditor;

namespace Game.LevelEditor
{
    /// <summary>
    /// Plays a saved level from the Level Editor (D75, D118): leaves the key for the bootstrapper and enters play mode,
    /// which always starts from the Bootstrap scene. The level is already addressable, saved just before.
    /// </summary>
    internal static class PlayLauncher
    {
        public static void Play(string levelKey)
        {
            new PlayRequest(new EditorPlayRequestStore()).Store(levelKey);
            EditorApplication.EnterPlaymode();
        }
    }
}
