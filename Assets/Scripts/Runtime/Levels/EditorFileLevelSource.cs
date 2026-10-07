#if UNITY_EDITOR
using System.IO;
using System.Threading.Tasks;
using Game.Core;
using Game.LevelIO;

namespace Game.Runtime
{
    /// <summary>
    /// Levels straight from their files, for the Level Editor's Play (D75): the level just saved plays without being
    /// added to any list. Editor only; I5 moves Play behind the bootstrapper, where every level comes by key.
    /// </summary>
    public sealed class EditorFileLevelSource : ILevelSource
    {
        /// <summary>Where the Level Editor saves levels: <c>&lt;key&gt;.json</c> (V1 D52).</summary>
        public const string Folder = "Assets/Levels";

        private readonly LevelJson json;

        public EditorFileLevelSource(LevelJson json) => this.json = json;

        public Task<Result<LevelData>> LoadAsync(string key)
        {
            string path = $"{Folder}/{key}.json";

            if (!File.Exists(path))
                return Task.FromResult(Result<LevelData>.Failure($"No level file at {path}."));

            return Task.FromResult(json.Parse(File.ReadAllText(path)));
        }
    }
}
#endif
