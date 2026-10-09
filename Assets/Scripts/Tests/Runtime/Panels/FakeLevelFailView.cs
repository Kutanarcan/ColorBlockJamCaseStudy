using System;
using Game.Runtime;
using UnityEngine;

namespace Game.Tests.Runtime
{
    /// <summary>Remembers what it shows; the test presses its buttons.</summary>
    internal sealed class FakeLevelFailView : ILevelFailView
    {
        public event Action ContinueClicked;

        public event Action CloseClicked;

        public event Action HoldStarted;

        public event Action HoldEnded;

        public string Title { get; private set; }

        public int Bonus { get; private set; }

        public int Price { get; private set; }

        public bool Affordable { get; private set; }

        public int Coins { get; private set; }

        public void ShowContent(string title, Sprite icon, int bonusSeconds, string description)
        {
            Title = title;
            Bonus = bonusSeconds;
        }

        public void ShowPrice(int price, bool affordable)
        {
            Price = price;
            Affordable = affordable;
        }

        public void ShowCoins(int coins) => Coins = coins;

        public void PressContinue() => ContinueClicked?.Invoke();

        public void PressClose() => CloseClicked?.Invoke();

        public void Hold() => HoldStarted?.Invoke();

        public void Release() => HoldEnded?.Invoke();
    }
}
