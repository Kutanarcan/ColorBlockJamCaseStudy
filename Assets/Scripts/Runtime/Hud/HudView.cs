using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay HUD (§5 HUD, view contract): level number, countdown, coins, and the restart and pause buttons it
    /// reports. A dumb view: it formats nothing but its own labels and knows no logic. The booster bar is static.
    /// Texts are set through TextMeshPro's number overloads, so an update makes no garbage.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private PressButton restartButton;
        [SerializeField] private PressButton pauseButton;

        public event Action RestartClicked
        {
            add => restartButton.Clicked += value;
            remove => restartButton.Clicked -= value;
        }

        public event Action PauseClicked
        {
            add => pauseButton.Clicked += value;
            remove => pauseButton.Clicked -= value;
        }

        public void SetLevel(int number) => levelText.SetText("{0}", number);

        public void SetTime(int minutes, int seconds) => timerText.SetText("{0:00}:{1:00}", minutes, seconds);

        public void SetCoins(int coins) => coinText.SetText("{0}", coins);
    }
}
