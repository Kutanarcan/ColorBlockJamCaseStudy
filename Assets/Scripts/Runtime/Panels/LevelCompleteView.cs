using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The LevelComplete panel (§5 view contract), placed inactive under the Gameplay scene's modal layer: the reward
    /// on its Continue button, and the press it reports. Title and coin pile are static.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelCompleteView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private PressButton continueButton;

        public event Action ContinueClicked
        {
            add => continueButton.Clicked += value;
            remove => continueButton.Clicked -= value;
        }

        public void ShowReward(int coins) => rewardText.SetText("{0}", coins);
    }
}
