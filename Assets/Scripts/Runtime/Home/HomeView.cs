using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Home (§5 view contract, D141), placed in the Main scene: the coins and the Settings button it reports. Lives,
    /// profile and tabs are static placeholders that only give the button feedback. The level button and the level
    /// track join in M2.3.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HomeView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private PressButton settingsButton;

        public event Action SettingsClicked
        {
            add => settingsButton.Clicked += value;
            remove => settingsButton.Clicked -= value;
        }

        public void SetCoins(int coins) => coinText.SetText("{0}", coins);
    }
}
