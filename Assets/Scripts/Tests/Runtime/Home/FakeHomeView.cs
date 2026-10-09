using System;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Remembers what it shows; the test presses its buttons.</summary>
    internal sealed class FakeHomeView : IHomeView
    {
        public event Action SettingsClicked;

        public event Action LevelClicked;

        public int Coins { get; private set; }

        public int Level { get; private set; }

        public int FirstComing { get; private set; }

        public void SetCoins(int coins) => Coins = coins;

        public void SetLevel(int levelNumber) => Level = levelNumber;

        public void SetComingLevels(int firstComing) => FirstComing = firstComing;

        public void PressLevel() => LevelClicked?.Invoke();

        public void PressSettings() => SettingsClicked?.Invoke();
    }
}
