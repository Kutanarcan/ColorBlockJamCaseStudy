using System.Collections.Generic;
using Game.Meta;
using UnityEngine;

namespace Game.Tests.Infrastructure
{
    /// <summary>Keeps each section as JSON text, as a file would between runs.</summary>
    internal sealed class FakeSaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> sections = new Dictionary<string, string>();

        public T Load<T>(string key) where T : class, new() =>
            sections.TryGetValue(key, out string json) ? JsonUtility.FromJson<T>(json) : new T();

        public void Save<T>(string key, T data) where T : class => sections[key] = JsonUtility.ToJson(data);
    }
}
