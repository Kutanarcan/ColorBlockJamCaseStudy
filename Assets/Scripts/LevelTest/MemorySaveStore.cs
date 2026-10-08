using System.Collections.Generic;
using Game.Meta;
using UnityEngine;

namespace Game.LevelTest
{
    /// <summary>
    /// Saved data that lives only as long as the run: the level test's store (D130). Sections are kept as JSON text, as a
    /// file would keep them, so a reader always gets a copy. It knows no other store; what it starts with is put in
    /// by whoever builds it.
    /// </summary>
    public sealed class MemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> sections = new Dictionary<string, string>();

        public T Load<T>(string key) where T : class, new() =>
            sections.TryGetValue(key, out string json) ? JsonUtility.FromJson<T>(json) : new T();

        public void Save<T>(string key, T data) where T : class => sections[key] = JsonUtility.ToJson(data);
    }
}
