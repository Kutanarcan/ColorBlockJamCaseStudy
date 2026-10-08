using System;
using System.IO;
using Game.Meta;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// Saved data as one JSON file per section, <c>&lt;key&gt;.json</c> in a folder the composition root puts under
    /// <c>persistentDataPath</c> (D128). Sections are independent: a file that cannot be read is reported and only
    /// that section starts fresh. A write goes to a temporary file first, so a crash mid-write never leaves half a
    /// section.
    /// </summary>
    public sealed class JsonSaveStore : ISaveStore
    {
        private readonly string folder;

        public JsonSaveStore(string folder) => this.folder = folder;

        /// <summary>Where the player's save lives on this device.</summary>
        public static string PlayerFolder => Path.Combine(Application.persistentDataPath, "Save");

        public T Load<T>(string key) where T : class, new()
        {
            string path = PathOf(key);

            if (!File.Exists(path))
                return new T();

            try
            {
                return JsonUtility.FromJson<T>(File.ReadAllText(path)) ?? new T();
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException)
            {
                Debug.LogWarning($"The save section '{key}' could not be read and starts fresh: {exception.Message}");

                return new T();
            }
        }

        public void Save<T>(string key, T data) where T : class
        {
            string path = PathOf(key);
            string temporaryPath = path + ".tmp";

            Directory.CreateDirectory(folder);
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));
            File.Copy(temporaryPath, path, true);
            File.Delete(temporaryPath);
        }

        private string PathOf(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("A save section needs a key.", nameof(key));

            return Path.Combine(folder, key + ".json");
        }
    }
}
