using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.LevelIO;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// Levels by key (D65): the level's JSON is a <c>TextAsset</c> loaded through the scope's asset loader, so it is
    /// released with that scope. An unknown key is a failed result, not an exception (D113).
    /// </summary>
    public sealed class AssetLevelSource : ILevelSource
    {
        private readonly IAssetLoader assets;
        private readonly LevelJson json;

        public AssetLevelSource(IAssetLoader assets, LevelJson json)
        {
            this.assets = assets;
            this.json = json;
        }

        public Task<Result<LevelData>> LoadAsync(string key) => LoadLevelAsync(key).AsTask();

        private async UniTask<Result<LevelData>> LoadLevelAsync(string key)
        {
            TextAsset level;

            try
            {
                level = await assets.LoadAsync<TextAsset>(key, CancellationToken.None);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                return Result<LevelData>.Failure($"No level with key '{key}': {exception.Message}");
            }

            return json.Parse(level.text);
        }
    }
}
