using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Keeps a content root inside the screen's safe area (D76). Goes on the content roots only, never on the dim or
    /// another full-screen backdrop: those cover the notch too, or a gap shows there. Its parent must fill the
    /// screen. Applied on enable and whenever the canvas resizes (rotation, Device Simulator), not every frame.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform rect;

        private void Awake() => rect = (RectTransform)transform;

        private void OnEnable() => Apply();

        private void OnRectTransformDimensionsChange() => Apply();

        private void Apply()
        {
            if (rect == null)
                return;

            Rect anchors = SafeAreaFit.Anchors(new Vector2(Screen.width, Screen.height), Screen.safeArea);

            // Setting the anchors resizes this rect and calls back here; equal anchors end it.
            if (rect.anchorMin == anchors.min && rect.anchorMax == anchors.max)
                return;

            rect.anchorMin = anchors.min;
            rect.anchorMax = anchors.max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
