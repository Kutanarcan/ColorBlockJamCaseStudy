using System;
using System.IO;
using UnityEditor;

namespace Game.LevelEditor
{
    /// <summary>
    /// Level files on disk: Assets/Levels/&lt;key&gt;.json. The key is the file name and the address; every written
    /// level is made addressable under it (D118).
    /// </summary>
    internal static class LevelFiles
    {
        public const string Folder = "Assets/Levels";

        public static string PathOf(string key) => $"{Folder}/{key}.json";

        public static bool Exists(string key) => File.Exists(PathOf(key));

        public static string[] Keys()
        {
            if (!Directory.Exists(Folder))
                return Array.Empty<string>();

            string[] files = Directory.GetFiles(Folder, "*.json");
            var keys = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                keys[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            Array.Sort(keys, StringComparer.Ordinal);

            return keys;
        }

        /// <summary>
        /// Asks for a new level's name in Unity's save dialog, which also confirms overwriting a file. False when
        /// cancelled (<paramref name="error"/> empty) or when the choice is not a valid level file in <see cref="Folder"/>.
        /// </summary>
        public static bool TryAskKey(out string key, out string error)
        {
            Directory.CreateDirectory(Folder);
            string path = EditorUtility.SaveFilePanelInProject("Save level", "level", "json",
                "Name the level. The name is its key: letters, digits, '-' and '_'.", Folder);
            key = Path.GetFileNameWithoutExtension(path);
            error = "";

            if (string.IsNullOrEmpty(path))
                return false;

            if (Path.GetDirectoryName(path)?.Replace('\\', '/') != Folder)
                error = $"Levels are saved in {Folder}.";
            else if (!LevelKey.IsValid(key))
                error = $"'{key}' is not a valid level name: use letters, digits, '-' and '_'.";

            return error.Length == 0;
        }

        public static string Read(string key) => File.ReadAllText(PathOf(key));

        public static void Write(string key, string json)
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathOf(key), json);
            AssetDatabase.ImportAsset(PathOf(key));
            LevelAddressables.Register(PathOf(key), key);
        }
    }
}
