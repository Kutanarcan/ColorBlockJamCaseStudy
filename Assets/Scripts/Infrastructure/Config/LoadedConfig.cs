using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure
{
    /// <summary>
    /// The run's <see cref="GameConfig"/>, loaded once through the root scope's asset loader, so it lives for the
    /// whole run. The root container is built before it can load, so the bootstrapper loads it before any content
    /// scene opens; reading it earlier is a bug and throws.
    /// </summary>
    public sealed class LoadedConfig
    {
        private readonly IAssetLoader assets;
        private GameConfig value;

        public LoadedConfig(IAssetLoader assets) => this.assets = assets;

        public GameConfig Value =>
            value ?? throw new InvalidOperationException("GameConfig was read before the bootstrapper loaded it.");

        public async UniTask LoadAsync(CancellationToken cancellation)
        {
            if (value != null)
                throw new InvalidOperationException("GameConfig is loaded once per run.");

            value = await assets.LoadAsync<GameConfig>(GameConfig.Key, cancellation);
        }
    }
}
