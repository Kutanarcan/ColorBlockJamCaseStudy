using System.Collections.Generic;
using Game.Meta;

namespace Game.Infrastructure
{
    /// <summary>The game's choice: the config's key where progression stands; it wraps after the last (D78).</summary>
    public sealed class ProgressionLevelChoice : ILevelChoice
    {
        private readonly LoadedConfig config;
        private readonly Progression progression;

        public ProgressionLevelChoice(LoadedConfig config, Progression progression)
        {
            this.config = config;
            this.progression = progression;
        }

        public string Choose()
        {
            IReadOnlyList<string> keys = config.Value.LevelKeys;

            return keys[progression.LevelIndex(keys.Count)];
        }
    }
}
