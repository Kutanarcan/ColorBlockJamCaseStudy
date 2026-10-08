using System;

namespace Game.Infrastructure
{
    /// <summary>
    /// The level the next Gameplay scene plays (D118). The bootstrapper selects the first one once the config is
    /// loaded; the next level after a win (U4) selects again. What is chosen is the start scene's
    /// <see cref="ILevelChoice"/> (D131).
    /// </summary>
    public sealed class SelectedLevel
    {
        private readonly ILevelChoice choice;
        private string key;

        public SelectedLevel(ILevelChoice choice) => this.choice = choice;

        public string Key => key ?? throw new InvalidOperationException("No level was selected yet.");

        public void Select() => key = choice.Choose();
    }
}
