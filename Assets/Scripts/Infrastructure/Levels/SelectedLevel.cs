using System;

namespace Game.Infrastructure
{
    /// <summary>
    /// The level the next Gameplay scene plays (D118). The bootstrapper chooses the first one: the Level Editor's
    /// play request if there is one, otherwise the first key in the config. Progression (M0) and the win popup's
    /// next level (M4) choose later ones.
    /// </summary>
    public sealed class SelectedLevel
    {
        private readonly LoadedConfig config;
        private readonly PlayRequest request;
        private string key;

        public SelectedLevel(LoadedConfig config, PlayRequest request)
        {
            this.config = config;
            this.request = request;
        }

        public string Key => key ?? throw new InvalidOperationException("No level was selected yet.");

        public void SelectFirst()
        {
            key = request.TryTake(out string requested) ? requested : config.Value.LevelKeys[0];
        }
    }
}
