using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime
{
    /// <summary>
    /// The LevelFail panel (§5 view contract), placed inactive under the Gameplay scene's modal layer: the fail kind's
    /// content, the continue price (red when the wallet cannot pay) and the coins, and the presses it reports. The
    /// lives counter is a static placeholder (D125). It carries the content of each fail kind; the scene picks one.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelFailView : MonoBehaviour, ILevelFailView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private Image icon;
        [SerializeField] private PressButton continueButton;
        [SerializeField] private PressButton closeButton;
        [SerializeField] private Color priceColor = Color.white;
        [SerializeField] private Color priceShortColor = Color.red;

        [Header("Fail kinds")]
        [SerializeField] private FailContent outOfTime = new FailContent();

        public FailContent OutOfTime => outOfTime;

        public event Action ContinueClicked
        {
            add => continueButton.Clicked += value;
            remove => continueButton.Clicked -= value;
        }

        public event Action CloseClicked
        {
            add => closeButton.Clicked += value;
            remove => closeButton.Clicked -= value;
        }

        public void ShowContent(string title, Sprite iconSprite, int bonusSeconds, string description)
        {
            titleText.SetText(title);

            // A content without its own icon keeps the one placed in the panel.
            if (iconSprite != null)
                icon.sprite = iconSprite;

            bonusText.SetText("+{0}", bonusSeconds);
            descriptionText.SetText(description, bonusSeconds);
        }

        public void ShowPrice(int price, bool affordable)
        {
            priceText.SetText("{0}", price);
            priceText.color = affordable ? priceColor : priceShortColor;
        }

        public void ShowCoins(int coins) => coinText.SetText("{0}", coins);
    }
}
