using System.Collections.Generic;
using Game.Meta;
using UnityEngine;

namespace Game.Tests.Meta
{
    /// <summary>
    /// Keeps each section as JSON text, as a file would between runs: a feature built on the same store reads a
    /// copy, never the object the last run changed. Counts the writes per key.
    /// </summary>
    internal sealed class FakeSaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> sections = new Dictionary<string, string>();
        private readonly Dictionary<string, int> writes = new Dictionary<string, int>();

        public T Load<T>(string key) where T : class, new() =>
            sections.TryGetValue(key, out string json) ? JsonUtility.FromJson<T>(json) : new T();

        public void Save<T>(string key, T data) where T : class
        {
            sections[key] = JsonUtility.ToJson(data);
            writes[key] = WritesOf(key) + 1;
        }

        public int WritesOf(string key) => writes.TryGetValue(key, out int count) ? count : 0;

        public bool Has(string key) => sections.ContainsKey(key);
    }
}
