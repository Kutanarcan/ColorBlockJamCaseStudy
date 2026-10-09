using System;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The LoseLife popup (§5 view contract): "Level X", the action button's label, and the action and close buttons it
    /// reports. The icon and the "You will lose 1 life!" text are static.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoseLifeView : MonoBehaviour, ILoseLifeView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI actionText;
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

        public void ShowTitle(int levelNumber) => titleText.SetText("Level {0}", levelNumber);

        public void ShowAction(string label) => actionText.SetText(label);
    }
}
