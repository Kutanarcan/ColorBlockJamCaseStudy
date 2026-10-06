using System;
using System.IO;
using UnityEditor;

namespace Game.LevelEditor
{
    /// <summary>Level files on disk: Assets/Levels/&lt;key&gt;.json. The key is the file name and the address.</summary>
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

        public static string Read(string key) => File.ReadAllText(PathOf(key));

        public static void Write(string key, string json)
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathOf(key), json);
            AssetDatabase.ImportAsset(PathOf(key));
        }
    }
}
