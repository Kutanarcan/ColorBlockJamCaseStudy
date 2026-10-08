#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Infrastructure
{
    /// <summary>
    /// Keeps the Level Editor's play request in the Editor's <c>SessionState</c>: it survives the domain reload of
    /// entering play mode and is gone when the Editor closes. A player build never has a request.
    /// </summary>
    public sealed class EditorPlayRequestStore : IPlayRequestStore
    {
#if UNITY_EDITOR
        private const string Key = "ColorBlockJam.PlayLevelKey";

        public string Read() => SessionState.GetString(Key, "");

        public void Write(string levelKey) => SessionState.SetString(Key, levelKey);
#else
        public string Read() => "";

        public void Write(string levelKey)
        {
        }
#endif
    }
}
