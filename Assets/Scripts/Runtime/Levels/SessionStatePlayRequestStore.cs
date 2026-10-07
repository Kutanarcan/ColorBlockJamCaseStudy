#if UNITY_EDITOR
using UnityEditor;

namespace Game.Runtime
{
    /// <summary>
    /// Keeps the play request in the Editor's <see cref="SessionState"/>: it survives the domain reload of entering
    /// play mode and is gone when the Editor closes. Editor only; a player build never has a request.
    /// </summary>
    public sealed class SessionStatePlayRequestStore : IPlayRequestStore
    {
        private const string Key = "ColorBlockJam.PlayLevelKey";

        public string Read() => SessionState.GetString(Key, "");

        public void Write(string levelKey) => SessionState.SetString(Key, levelKey);
    }
}
#endif
