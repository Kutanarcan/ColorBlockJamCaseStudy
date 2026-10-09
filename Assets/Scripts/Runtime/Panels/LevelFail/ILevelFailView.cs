using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>What the LevelFail presenter drives: the fail kind's content, the continue price, the coins.</summary>
    public interface ILevelFailView
    {
        event Action ContinueClicked;

        event Action CloseClicked;

        /// <summary><paramref name="description"/> may hold <c>{0}</c>, the bonus seconds.</summary>
        void ShowContent(string title, Sprite icon, int bonusSeconds, string description);

        /// <summary>The price in red when the wallet cannot pay it (D124).</summary>
        void ShowPrice(int price, bool affordable);

        void ShowCoins(int coins);
    }
}
