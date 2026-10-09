using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The Play popup (§5 view contract): its title, the heart beside it (the title's layout group places what is
    /// shown), the action button's label, and the action and close buttons it reports. The booster row is static.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayView : MonoBehaviour, IPlayView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private GameObject titleIcon;
        [SerializeField] private PressButton actionButton;
        [SerializeField] private PressButton closeButton;

        public event Action ActionClicked
        {
            add => actionButton.Clicked += value;
            remove => actionButton.Clicked -= value;
        }

        public event Action CloseClicked
        {
            add => closeButton.Clicked += value;
            remove => closeButton.Clicked -= value;
        }

        public void ShowTitle(string title) => titleText.SetText(title);

        public void ShowTitleIcon(bool shown) => titleIcon.SetActive(shown);

        public void ShowAction(string label) => actionText.SetText(label);
    }
}
