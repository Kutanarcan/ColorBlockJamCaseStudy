using System;

namespace Game.Infrastructure
{
    /// <summary>
    /// The run's <see cref="GameConfig"/>. The root container is built before the config can be loaded, so the
    /// bootstrapper sets it once, before any content scene opens; reading it earlier is a bug and throws.
    /// </summary>
    public sealed class LoadedConfig
    {
        private GameConfig value;

        public GameConfig Value =>
            value ?? throw new InvalidOperationException("GameConfig was read before the bootstrapper loaded it.");

        public void Set(GameConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (value != null)
                throw new InvalidOperationException("GameConfig is loaded once per run.");

            value = config;
        }
    }
}
