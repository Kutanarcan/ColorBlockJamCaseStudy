using System;

namespace Game.Runtime
{
    /// <summary>What the Home presenter drives: the coins, the current and coming levels, and two buttons.</summary>
    public interface IHomeView
    {
        event Action SettingsClicked;

        event Action LevelClicked;

        void SetCoins(int coins);

        void SetLevel(int levelNumber);

        /// <summary>Numbers the level track's bubbles from <paramref name="firstComing"/> on (D141).</summary>
        void SetComingLevels(int firstComing);
    }
}
