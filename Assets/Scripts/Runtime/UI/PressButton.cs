using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Runtime
{
    /// <summary>
    /// The one button every screen, panel and popup uses (D76), without Unity's <c>Button</c>. It sits alone on an
    /// empty object with two child images: <see cref="background"/> (raycast target off; it shrinks while pressed,
    /// with what sits on it, and springs back on release) and <see cref="raycast"/> (the hit area, never moves). The
    /// raycast image's Raycast Target is the button's on / off: on, the button answers; off, it neither shrinks nor
    /// clicks. Set it in the Editor to start a button off. The child's pointer events reach this object through the
    /// parent chain. A click is a release over the button that was pressed; dragging off cancels it. The feedback
    /// answers the pointer, not the action, so a placeholder or a button whose action does nothing (LevelFail's
    /// Continue without coins) still answers. Runs on unscaled time and leaves the background at its rest scale when
    /// disabled.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Tooltip("Child image that shrinks while pressed; its raycast target is turned off.")]
        [SerializeField] private Image background;

        [Tooltip("Child image that takes the touches and never moves; its Raycast Target turns the button on / off.")]
        [SerializeField] private Image raycast;

        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.92f;
        [SerializeField, Min(0f)] private float pressDuration = 0.06f;
        [SerializeField, Min(0f)] private float releaseDuration = 0.12f;

        private bool prepared;
        private RectTransform target;
        private Vector3 restScale;
        private DOGetter<Vector3> getScale;
        private DOSetter<Vector3> setScale;

        /// <summary>Raised on a click; the view that owns the button listens.</summary>
        public event Action Clicked;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Prepare())
                return;

            ScaleTo(restScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!prepared)
                return;

            ScaleTo(restScale, releaseDuration, Ease.OutBack);
        }

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();

        private void OnDisable()
        {
            if (!prepared)
                return;

            target.DOKill();
            target.localScale = restScale;
        }

        /// <summary>Only the raycast image takes touches; its own Raycast Target is left as set.</summary>
        private void OnValidate()
        {
            if (background != null)
                background.raycastTarget = false;
        }

        /// <summary>
        /// Once, on the first press: the rest scale and the getter / setter, made once like <see cref="BlockView"/>'s
        /// (D109). Without a background there is no feedback; the click still works.
        /// </summary>
        private bool Prepare()
        {
            if (prepared)
                return true;

            if (background == null)
            {
                Debug.LogError($"{name}: PressButton has no background.", this);
                return false;
            }

            target = background.rectTransform;
            restScale = target.localScale;
            getScale = GetScale;
            setScale = SetScale;
            prepared = true;

            return true;
        }

        /// <summary>No per-press garbage: the tween is recyclable and found again only through its target (D109).</summary>
        private void ScaleTo(Vector3 scale, float duration, Ease ease)
        {
            target.DOKill();
            DOTween.To(getScale, setScale, scale, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .SetTarget(target)
                .SetLink(target.gameObject)
                .SetRecyclable(true);
        }

        private Vector3 GetScale() => target.localScale;

        private void SetScale(Vector3 scale) => target.localScale = scale;
    }
}
