using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Prototype
{
    // A hand sprite that follows the mouse, for screen recordings (the OS cursor is not captured).
    // The sprite's own pivot (set to the fingertip) is the hotspot.
    [Serializable]
    public class HandCursor
    {
        [SerializeField] bool show = true;             // toggle in the Inspector, also during Play
        [SerializeField] Sprite sprite;
        [SerializeField] float size = 160f;            // on-screen height in pixels
        [SerializeField] bool hideSystemCursor = true; // hide the OS cursor while the hand is shown

        RectTransform rect;
        Image image;

        public void Init()
        {
            if (sprite == null) return;

            var canvas = new GameObject("HandCursorCanvas", typeof(Canvas)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue; // above any other UI

            image = new GameObject("HandCursor", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(canvas.transform, false);
            image.sprite = sprite;
            image.raycastTarget = false;

            // Anchored to the screen's bottom-left, so anchoredPosition = Input.mousePosition in pixels.
            // Image ignores the sprite pivot, so copy it onto the RectTransform.
            rect = (RectTransform)image.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = sprite.pivot / sprite.rect.size;
        }

        public void Tick()
        {
            if (image == null) return;

            image.enabled = show;
            if (hideSystemCursor) Cursor.visible = !show;

            rect.sizeDelta = new Vector2(size * sprite.rect.width / sprite.rect.height, size);
            rect.anchoredPosition = Input.mousePosition;
        }
    }
}
