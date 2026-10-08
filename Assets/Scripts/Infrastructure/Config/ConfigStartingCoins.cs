using Game.Meta;

namespace Game.Infrastructure
{
    /// <summary>The game's starting coins, read from the config when a fresh wallet first opens (after start-up).</summary>
    public sealed class ConfigStartingCoins : IStartingCoins
    {
        private readonly LoadedConfig config;

        public ConfigStartingCoins(LoadedConfig config) => this.config = config;

        public int Amount => config.Value.StartCoins;
    }
}
