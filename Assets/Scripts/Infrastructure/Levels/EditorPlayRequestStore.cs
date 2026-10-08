#if UNITY_EDITOR
using UnityEditor;

namespace Game.Infrastructure
{
    /// <summary>
    /// Keeps the Level Editor's play request in the Editor's <see cref="SessionState"/>: it survives the domain reload
    /// of entering play mode and is gone when the Editor closes. Editor only; a player build gets
    /// <see cref="NoPlayRequests"/>.
    /// </summary>
    public sealed class EditorPlayRequestStore : IPlayRequestStore
    {
        private const string Key = "ColorBlockJam.PlayLevelKey";

        public string Read() => SessionState.GetString(Key, "");

        public void Write(string levelKey) => SessionState.SetString(Key, levelKey);
    }
}
#endif
