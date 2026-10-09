using System;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The Settings popup (§5 view contract, D126): three toggles, Home and close. A dumb view: it reports its buttons
    /// and shows what it is told. Legal Terms, Restore, Support and Language are placeholders that only give the
    /// button feedback, so it holds no reference to them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] private ToggleView vibration;
        [SerializeField] private ToggleView sound;
        [SerializeField] private ToggleView music;
        [SerializeField] private PressButton homeButton;
        [SerializeField] private PressButton closeButton;

        public ToggleView Vibration => vibration;

        public ToggleView Sound => sound;

        public ToggleView Music => music;

        public event Action HomeClicked
        {
            add => homeButton.Clicked += value;
            remove => homeButton.Clicked -= value;
        }

        public event Action CloseClicked
        {
            add => closeButton.Clicked += value;
            remove => closeButton.Clicked -= value;
        }

        /// <summary>Gameplay shows the Home button; Home itself does not (D126).</summary>
        public void ShowHome(bool shown) => homeButton.gameObject.SetActive(shown);
    }
}
