using System;
using System.Collections.Generic;
using Game.Meta;

namespace Game.Infrastructure
{
    /// <summary>
    /// The level the next Gameplay scene plays (D118). The bootstrapper chooses the first one: the Level Editor's
    /// play request if there is one, otherwise the config's key where progression stands (it wraps, D78). The next
    /// level after a win (U4) chooses again.
    /// </summary>
    public sealed class SelectedLevel
    {
        private readonly LoadedConfig config;
        private readonly PlayRequest request;
        private readonly Progression progression;
        private string key;

        public SelectedLevel(LoadedConfig config, PlayRequest request, Progression progression)
        {
            this.config = config;
            this.request = request;
            this.progression = progression;
        }

        public string Key => key ?? throw new InvalidOperationException("No level was selected yet.");

        public void SelectFirst()
        {
            key = request.TryTake(out string requested) ? requested : ProgressionKey();
        }

        private string ProgressionKey()
        {
            IReadOnlyList<string> keys = config.Value.LevelKeys;

            return keys[progression.LevelIndex(keys.Count)];
        }
    }
}
