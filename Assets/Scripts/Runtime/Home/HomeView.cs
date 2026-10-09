using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Home (§5 view contract, D141), placed in the Main scene: the coins, the level button with the current level,
    /// and the level track, a scroll view of the coming levels (3–4 in sight) whose bubbles are numbered in order.
    /// Lives, profile and tabs are static placeholders that only give the button feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HomeView : MonoBehaviour, IHomeView
    {
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private PressButton levelButton;
        [SerializeField] private PressButton settingsButton;

        [Tooltip("The level track's bubble numbers, nearest first; the track scrolls through them.")]
        [SerializeField] private TextMeshProUGUI[] comingLevelTexts = Array.Empty<TextMeshProUGUI>();

        public event Action SettingsClicked
        {
            add => settingsButton.Clicked += value;
            remove => settingsButton.Clicked -= value;
        }

        public event Action LevelClicked
        {
            add => levelButton.Clicked += value;
            remove => levelButton.Clicked -= value;
        }

        public void SetCoins(int coins) => coinText.SetText("{0}", coins);

        public void SetLevel(int levelNumber) => levelText.SetText("Level {0}", levelNumber);

        public void SetComingLevels(int firstComing)
        {
            for (int i = 0; i < comingLevelTexts.Length; i++)
                comingLevelTexts[i].SetText("{0}", firstComing + i);
        }
    }
}
