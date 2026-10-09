using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime
{
    /// <summary>
    /// The on / off switch part (§5 Parts): a <see cref="PressButton"/> whose background holds a track and a knob.
    /// On → off the knob slides from <see cref="onX"/> to <see cref="offX"/>, its sprite swaps and the track's color
    /// swaps; and back. It reports the press and shows what it is told; what the switch means is its owner's business.
    /// The slide runs on unscaled time and makes no garbage (D109).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ToggleView : MonoBehaviour
    {
        [SerializeField] private PressButton button;
        [SerializeField] private Image track;
        [SerializeField] private Image knob;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Color onColor = new Color(0.18f, 0.67f, 0f);
        [SerializeField] private Color offColor = Color.white;

        [Tooltip("The knob's anchored x when on.")]
        [SerializeField] private float onX;

        [Tooltip("The knob's anchored x when off.")]
        [SerializeField] private float offX;

        [SerializeField, Min(0f)] private float slideDuration = 0.15f;

        private RectTransform knobRect;
        private DOGetter<float> getKnobX;
        private DOSetter<float> setKnobX;

        public event Action Clicked
        {
            add => button.Clicked += value;
            remove => button.Clicked -= value;
        }

        /// <summary>Shows on or off; slides the knob when <paramref name="animate"/>, else puts it there at once.</summary>
        public void Show(bool on, bool animate)
        {
            Prepare();
            track.color = on ? onColor : offColor;
            knob.sprite = on ? onSprite : offSprite;
            float x = on ? onX : offX;
            knobRect.DOKill();

            if (!animate)
            {
                SetKnobX(x);
                return;
            }

            DOTween.To(getKnobX, setKnobX, x, slideDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetTarget(knobRect)
                .SetLink(knobRect.gameObject)
                .SetRecyclable(true);
        }

        private void OnDisable()
        {
            if (knobRect != null)
                knobRect.DOKill(complete: true);
        }

        /// <summary>Once: a popup's toggles are shown while it is still off, before any Awake has run.</summary>
        private void Prepare()
        {
            if (knobRect != null)
                return;

            knobRect = knob.rectTransform;
            getKnobX = GetKnobX;
            setKnobX = SetKnobX;
        }

        private float GetKnobX() => knobRect.anchoredPosition.x;

        private void SetKnobX(float x)
        {
            Vector2 position = knobRect.anchoredPosition;
            position.x = x;
            knobRect.anchoredPosition = position;
        }
    }
}
