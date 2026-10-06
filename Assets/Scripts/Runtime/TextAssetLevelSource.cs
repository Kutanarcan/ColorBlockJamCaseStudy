using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Core;
using Game.LevelIO;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Levels from TextAssets referenced by the scene; the key is the asset name, as the editor saves it (D52).
    /// Replaced by the Addressables source in I4; callers only see <see cref="ILevelSource"/>.
    /// </summary>
    public sealed class TextAssetLevelSource : ILevelSource
    {
        private readonly IReadOnlyList<TextAsset> levels;
        private readonly LevelJson json;

        public TextAssetLevelSource(IReadOnlyList<TextAsset> levels, LevelJson json)
        {
            this.levels = levels;
            this.json = json;
        }

        public Task<Result<LevelData>> LoadAsync(string key)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].name == key)
                    return Task.FromResult(json.Parse(levels[i].text));
            }

            return Task.FromResult(Result<LevelData>.Failure($"No level with key '{key}'."));
        }
    }
}
