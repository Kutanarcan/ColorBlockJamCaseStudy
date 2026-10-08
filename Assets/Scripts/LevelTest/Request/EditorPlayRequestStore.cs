using UnityEditor;

namespace Game.LevelTest
{
    /// <summary>
    /// Keeps the Level Editor's play request in the Editor's <see cref="SessionState"/>: it survives the domain reload
    /// of entering play mode and is gone when the Editor closes.
    /// </summary>
    public sealed class EditorPlayRequestStore : IPlayRequestStore
    {
        private const string Key = "ColorBlockJam.PlayLevelKey";

        public string Read() => SessionState.GetString(Key, "");

        public void Write(string levelKey) => SessionState.SetString(Key, levelKey);
    }
}
